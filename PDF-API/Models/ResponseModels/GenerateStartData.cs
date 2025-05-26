using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class GenerateStartData
{
    [JsonPropertyName("byteUnit")]
    public string ByteUnit { get; set; } = string.Empty;
}
