namespace PDF_API.Models.RequestModels
{
    public class RandomRequestModel
    {
        public int SizeMin { get; set; }
        public int SizeMax { get; set; }
        public int PageMin { get; set; }
        public int PageMax { get; set; }
        public string Mode { get; set; } = "single";
        public int? NumberOfFiles { get; set; }
        public string ByteUnit { get; set; }
        public string? MetricUnit { get; set; }
        public string? User { get; set; }
    }
}
