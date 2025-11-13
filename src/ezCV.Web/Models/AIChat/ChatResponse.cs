namespace ezCV.Web.Models.AIChat
{
    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public string SessionGuid { get; set; } = string.Empty;
        public List<AiSuggestion> Suggestions { get; set; } = new();
        public GeneratedContent? GeneratedContent { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
