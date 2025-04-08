using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using PDF_API.Adapters;

namespace PDF_API;

public class MessagingService : IAsyncDisposable
{
    private const int HistorySize = 500;

    private readonly ConcurrentDictionary<string, IConnectionAdapter> Connections = new();
    private readonly ConcurrentQueue<Message> History = new();
    private readonly ILogger<MessagingService> _logger;

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

        await SendMessage(connection, new History(History.TakeLast(100)));
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

    public ValueTask DisposeAsync()
    {
        // No resources to dispose
        return ValueTask.CompletedTask;
    }
}