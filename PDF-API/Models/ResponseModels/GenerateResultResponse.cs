using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class GenerateResultResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("data")]
    public GenerateResultData? Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("ProcessId")]
    public string? ProcessId { get; set; }  // Nullable process ID for async requests
}
