using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class GenerateStartData
{
    [JsonPropertyName("estimatedSize")]
    public long EstimatedSize { get; set; }

    [JsonPropertyName("byteUnit")]
    public string ByteUnit { get; set; } = string.Empty;
}
