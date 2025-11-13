namespace ezCV.Web.Models.AIChat
{
    public class AiSuggestion
    {
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TargetField { get; set; }
        public string? Example { get; set; }
        public string? Icon { get; set; }
    }
}
