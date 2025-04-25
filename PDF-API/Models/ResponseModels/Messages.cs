using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

// Required for polymorphic deserialization
[JsonDerivedType(typeof(ProcessStarted), typeDiscriminator: "ProcessStarted")]
[JsonDerivedType(typeof(ProcessProgress), typeDiscriminator: "ProcessProgress")]
[JsonDerivedType(typeof(ProcessCompleted), typeDiscriminator: "ProcessCompleted")]
[JsonDerivedType(typeof(ProcessFailed), typeDiscriminator: "ProcessFailed")]
[JsonDerivedType(typeof(UserList), typeDiscriminator: "UserList")]
[JsonDerivedType(typeof(UserConnected), typeDiscriminator: "UserConnected")]
[JsonDerivedType(typeof(UserDisconnected), typeDiscriminator: "UserDisconnected")]
[JsonDerivedType(typeof(History), typeDiscriminator: "History")]
[JsonDerivedType(typeof(PingMessage), typeDiscriminator: "PingMessage")]
[JsonDerivedType(typeof(PongMessage), typeDiscriminator: "PongMessage")]
[JsonDerivedType(typeof(Error), typeDiscriminator: "Error")]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
public record Message(string Type);

// Process-related messages
public record ProcessStarted(string ProcessId, string ProcessName, string Initiator) : Message(nameof(ProcessStarted))
{
    public DateTimeOffset StartTime { get; init; } = DateTimeOffset.UtcNow;
    public string Status { get; init; } = "Started";
    public Dictionary<string, object>? AdditionalData { get; init; }
}

public record ProcessProgress(string ProcessId, int PercentComplete) : Message(nameof(ProcessProgress))
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? CurrentStage { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
}

public record ProcessCompleted(string ProcessId) : Message(nameof(ProcessCompleted))
{
    public DateTimeOffset CompletionTime { get; init; } = DateTimeOffset.UtcNow;
    public TimeSpan Duration { get; init; }
    public string? ResultUrl { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }

    public ProcessCompleted(string processId, DateTimeOffset startTime) : this(processId)
    {
        Duration = CompletionTime - startTime;
    }
}

public record ProcessFailed(string ProcessId, string ErrorMessage) : Message(nameof(ProcessFailed))
{
    public DateTimeOffset FailureTime { get; init; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; init; }
}

// Keep existing user management messages
public record UserList(IEnumerable<string> Users) : Message(nameof(UserList));

public record UserConnected(string Name, string Transport) : Message(nameof(UserConnected));

public record UserDisconnected(string Name) : Message(nameof(UserDisconnected));

public record History(IEnumerable<Message> Messages) : Message(nameof(History));

public record Error(string ErrorMessage) : Message(nameof(Error));

// Keep ping/pong messages for connection monitoring
public record PingMessage() : Message(nameof(PingMessage))
{
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

public record PongMessage() : Message(nameof(PongMessage))
{
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public long OriginalTimestamp { get; init; }

    public PongMessage(long originalTimestamp) : this()
    {
        OriginalTimestamp = originalTimestamp;
    }
}