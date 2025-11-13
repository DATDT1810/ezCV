using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
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
