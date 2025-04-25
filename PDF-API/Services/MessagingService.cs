using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using PDF_API.Adapters;
using PDF_API.Models;
using PDF_API.Models.ResponseModels;

namespace PDF_API.Services;

public class MessagingService : IAsyncDisposable
{
    private const int HistorySize = 500;

    private readonly ConcurrentDictionary<string, IConnectionAdapter> Connections = new();
    private readonly ConcurrentQueue<Message> History = new();
    private readonly ILogger<MessagingService> _logger;

    private readonly ConcurrentDictionary<string, ProcessStarted> ActiveProcesses = new();

    public MessagingService(ILogger<MessagingService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> TryAddUser(IConnectionAdapter connection, string name)
    {
        if (!TryAddUser(name, connection))
        {
            return $"Name '{name}' already taken";
        }

        var userConnected = new UserConnected(name, GetTransport(connection));
        var everyoneElse = Connections.Where(x => x.Key != name).Select(x => x.Value);
        await BroadcastMessage(userConnected, everyoneElse);

        if (ActiveProcesses.Count > 0)
        {
            var activeProcessMessages = new List<Message>();

            foreach (var process in ActiveProcesses.Values)
            {
                activeProcessMessages.Add(process);
            }

            await SendMessage(connection, new History(activeProcessMessages));
        }
        else
        {
            await SendMessage(connection, new History(Enumerable.Empty<Message>()));
        }

        await SendMessage(connection, new UserList(Connections.Keys));

        return null;
    }

    private string GetTransport(IConnectionAdapter conn) => conn switch
    {
        WebSocketAdapter _ => "WebSocket",
        _ => "Unknown transport"
    };

    private bool TryAddUser(string name, IConnectionAdapter connection)
    {
        if (Connections.ContainsKey(name))
        {
            return false;
        }

        Connections.TryAdd(name, connection);

        return true;
    }

    public Task RemoveUser(string name)
    {
        Connections.TryRemove(name, out _);
        var msg = new UserDisconnected(name);
        return BroadcastMessage(msg);
    }

    public Task SendMessage(IConnectionAdapter connection, Message message)
    {
        _logger.LogInformation("Sending message: {message}", message);

        return connection.SendMessage(message);
    }

    public async Task BroadcastMessage(Message message, IEnumerable<IConnectionAdapter>? receivers = null)
    {
        _logger.LogInformation("Broadcasting message: {message}", message);

        History.Enqueue(message);

        // Limit history size
        while (History.Count > HistorySize)
        {
            History.TryDequeue(out _);
        }

        foreach (var connection in receivers ?? Connections.Values)
        {
            await connection.SendMessage(message);
        }
    }

    public Task StartProcess(string processId, string processName, string initiator, Dictionary<string, object>? additionalData = null)
    {
        var processStarted = new ProcessStarted(processId, processName, initiator)
        {
            AdditionalData = additionalData
        };

        ActiveProcesses.TryAdd(processId, processStarted);
        return BroadcastMessage(processStarted);
    }

    public Task UpdateProcessProgress(string processId, int percentComplete, string? currentStage = null, Dictionary<string, object>? additionalData = null)
    {
        var progress = new ProcessProgress(processId, percentComplete)
        {
            CurrentStage = currentStage,
            AdditionalData = additionalData
        };

        return BroadcastMessage(progress);
    }

    public Task CompleteProcess(string processId, string? resultUrl = null, Dictionary<string, object>? additionalData = null)
    {
        if (ActiveProcesses.TryRemove(processId, out var processStarted))
        {
            var completed = new ProcessCompleted(processId, processStarted.StartTime)
            {
                ResultUrl = resultUrl,
                AdditionalData = additionalData
            };

            return BroadcastMessage(completed);
        }

        return Task.CompletedTask;
    }

    public Task FailProcess(string processId, string errorMessage, Dictionary<string, object>? additionalData = null)
    {
        ActiveProcesses.TryRemove(processId, out _);

        var failed = new ProcessFailed(processId, errorMessage)
        {
            AdditionalData = additionalData
        };

        return BroadcastMessage(failed);
    }

    public ValueTask DisposeAsync()
    {
        // No resources to dispose
        return ValueTask.CompletedTask;
    }
}