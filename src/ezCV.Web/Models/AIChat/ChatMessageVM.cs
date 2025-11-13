namespace ezCV.Web.Models.AIChat
{
    public class ChatMessageVM
    {
        public string Content { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<AiSuggestion> Suggestions { get; set; } = new List<AiSuggestion>();
        public bool IsTyping { get; set; }
    }
}
