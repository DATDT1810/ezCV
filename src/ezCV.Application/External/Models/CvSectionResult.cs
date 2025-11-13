using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.External.Models
{
    public class CvSectionResult
    {
        public string Content { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
    }
}
