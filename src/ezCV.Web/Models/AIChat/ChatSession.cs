namespace ezCV.Web.Models.AIChat
{
    public class ChatSession
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int MessageCount { get; set; }

        // For display
        public string StartedAtDisplay => StartedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        public string Duration => CompletedAt.HasValue
            ? $"{(CompletedAt.Value - StartedAt).TotalMinutes:F0} phút"
            : "Đang hoạt động";
    }
}
