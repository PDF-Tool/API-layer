using System.Text.Json.Serialization;

namespace PDF_API.Models.ResponseModels;

public class GenerateStartResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("data")]
    public GenerateStartData? Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
