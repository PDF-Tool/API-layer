using System;

namespace PDF_API.Models.RequestModels;

public class PerformanceRunRequestModel
{
    public int AmountOfTime { get; set; }
    public int? PagesPerFile { get; set; }
    public int? SizePerPage { get; set; }
    public string? ByteUnit { get; set; }
    public string? MetricUnit { get; set; }
    public string? User { get; set; }
    public required string Host { get; set; }
}
