using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PDF_API.Models.ResponseModels;
using PDF_API.Services;

namespace PDF_API.Adapters;

public class WebSocketAdapter : IConnectionAdapter
{
    private readonly MessagingService _messagingService;
    private WebSocket? _webSocket;
    private string? _userName;
    private readonly ILogger<WebSocketAdapter> _logger;
    private CancellationTokenSource _pingCts = new CancellationTokenSource();

    public WebSocketAdapter(MessagingService messagingService, ILogger<WebSocketAdapter> logger)
    {
        _messagingService = messagingService;
        _logger = logger;
    }

    public async Task HandleUser(HttpContext context, string name)
    {
        _userName = name;
        _webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var error = await _messagingService.TryAddUser(this, name);
        if (error != null)
        {
            await SendMessage(new Error(error));
            await _webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, error, CancellationToken.None);
            return;
        }

        // Start ping task to keep connection alive
        _ = StartPingAsync();

        // Listen for incoming messages from the client
        try
        {
            var buffer = new byte[4096];
            var receiveResult = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    await HandleMessageAsync(message);
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                receiveResult = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // Client closed connection
            await _webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "WebSocket error occurred for user {UserName}", _userName);
        }
        finally
        {
            _pingCts.Cancel();
            if (_userName != null)
            {
                await _messagingService.RemoveUser(_userName);
            }
        }
    }

    private async Task StartPingAsync()
    {
        try
        {
            while (!_pingCts.Token.IsCancellationRequested && _webSocket != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), _pingCts.Token);

                if (_webSocket.State == WebSocketState.Open)
                {
                    // Send a ping message as JSON
                    await SendMessage(new PingMessage());
                }
                else
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the token is canceled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ping loop for user {UserName}", _userName);
        }
    }

    private Task HandleMessageAsync(string messageJson)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(messageJson);
            var messageType = jsonDoc.RootElement.GetProperty("Type").GetString();

            switch (messageType)
            {
                case "ProcessProgress":
                    var progressMessage = JsonSerializer.Deserialize<ProcessProgress>(messageJson);
                    return _messagingService.BroadcastMessage(progressMessage!);

                case "ProcessStarted":
                    var startedMessage = JsonSerializer.Deserialize<ProcessStarted>(messageJson);
                    return _messagingService.BroadcastMessage(startedMessage!);

                case "ProcessCompleted":
                    var completedMessage = JsonSerializer.Deserialize<ProcessCompleted>(messageJson);
                    return _messagingService.BroadcastMessage(completedMessage!);

                case "ProcessFailed":
                    var failedMessage = JsonSerializer.Deserialize<ProcessFailed>(messageJson);
                    return _messagingService.BroadcastMessage(failedMessage!);

                case "PongMessage":
                    // Client responded to ping, connection is alive
                    return Task.CompletedTask;

                default:
                    _logger.LogWarning("Unknown message type received: {MessageType}", messageType);
                    return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message: {Message}", messageJson);
            return Task.CompletedTask;
        }
    }

    public async Task SendMessage(Message message)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to user {UserName}", _userName);
        }
    }
}