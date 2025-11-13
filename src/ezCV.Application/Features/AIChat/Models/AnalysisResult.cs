using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Models
{
    public class AnalysisResult
    {
        public bool IsDataComplete { get; set; }
        public List<string> MissingInformation { get; set; } = new List<string>();

    }
}
