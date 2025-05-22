namespace PDF_API.Models.RequestModels;

public class BatchRequestModel
{
    public int? NumberOfFiles { get; set; }
    public int? PagesPerFile { get; set; }
    public int? SizePerPage { get; set; }
    public string? ByteUnit { get; set; }
    public string? MetricUnit { get; set; }
    public string? User { get; set; }
    public required string Host { get; set; }
} 