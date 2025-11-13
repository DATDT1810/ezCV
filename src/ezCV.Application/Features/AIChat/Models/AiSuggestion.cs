using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
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
