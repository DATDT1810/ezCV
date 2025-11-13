namespace ezCV.Web.Models.AIChat
{
    public class ChatViewModel
    {
        public string? SessionGuid { get; set; }
        public List<ChatMessageVM> Messages { get; set; } = new List<ChatMessageVM>();
        public bool IsActive { get; set; }
        public string? CurrentInput { get; set; }
        public bool IsLoading { get; set; }
    }
}
