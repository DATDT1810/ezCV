namespace ezCV.Web.Models.AIChat
{
    public class CvGenerationResult
    {
        public string Content { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public long GenerationId { get; set; }
    }
}
