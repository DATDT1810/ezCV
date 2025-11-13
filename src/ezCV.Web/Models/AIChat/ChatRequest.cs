namespace ezCV.Web.Models.AIChat
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionGuid { get; set; }
        public string? Context { get; set; }
    }
}
