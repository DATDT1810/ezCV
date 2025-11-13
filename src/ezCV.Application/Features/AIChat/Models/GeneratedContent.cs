using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
{
    public class GeneratedContent
    {
        public string Section { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Reasoning { get; set; }
        public decimal Confidence { get; set; }
    }
}
