namespace ezCV.Web.Models.CvProcess
{
    public class CvProcessResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? GeneratedPdfUrl { get; set; }
        public string? PreviewHtml { get; set; }
        public int CvId { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
