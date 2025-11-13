using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
{
    public class CvGenerationResult
    {
        public string Content { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public long GenerationId { get; set; }
    }
}
