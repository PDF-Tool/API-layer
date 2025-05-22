using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class BatchGenerateStartData
{
    [JsonPropertyName("byteUnit")]
    public string ByteUnit { get; set; } = string.Empty;

    [JsonPropertyName("numberOfFiles")]
    public int NumberOfFiles { get; set; }

    [JsonPropertyName("pagesPerFile")]
    public int PagesPerFile { get; set; }
} 