using System.Text.Json.Serialization;

namespace PDF_API;

// Required for polymorphic deserialization
[JsonDerivedType(typeof(ChatMessage), typeDiscriminator: "ChatMessage")]
[JsonDerivedType(typeof(UserList), typeDiscriminator: "UserList")]
[JsonDerivedType(typeof(UserConnected), typeDiscriminator: "UserConnected")]
[JsonDerivedType(typeof(UserDisconnected), typeDiscriminator: "UserDisconnected")]
[JsonDerivedType(typeof(History), typeDiscriminator: "History")]
[JsonDerivedType(typeof(PingMessage), typeDiscriminator: "PingMessage")]
[JsonDerivedType(typeof(PongMessage), typeDiscriminator: "PongMessage")]
[JsonDerivedType(typeof(Error), typeDiscriminator: "Error")]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
public record Message(string Type);

public record ChatMessage(string Name, string Content) : Message(nameof(ChatMessage));

public record UserList(IEnumerable<string> Users) : Message(nameof(UserList));

public record UserConnected(string Name, string Transport) : Message(nameof(UserConnected));

public record UserDisconnected(string Name) : Message(nameof(UserDisconnected));

public record History(IEnumerable<Message> Messages) : Message(nameof(History));

public record Error(string ErrorMessage) : Message(nameof(Error));

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