using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class GenerateResultData
{
    [JsonPropertyName("actualSize")]
    public long ActualSize { get; set; }

    [JsonPropertyName("byteUnit")]
    public string ByteUnit { get; set; } = string.Empty;
}
