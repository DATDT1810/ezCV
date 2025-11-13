using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
{
    public class ChatSession
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
