using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionGuid { get; set; }
        public string? Context { get; set; }
    }
}
