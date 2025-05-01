namespace PDF_API.Models.RequestModels;

public class RequestModel
{
    public int? Pages { get; set; }
    public int? SizePerPage { get; set; }
    public string? ByteUnit { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? MetricUnit { get; set; }
    public int? Square { get; set; }
    public string? Format { get; set; }

    public string? User { get; set; }
    public bool? IsRandom { get; set; }
}