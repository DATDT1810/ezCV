using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.External.Models
{
    public class AiAnalysisResult
    {
        public List<ExtractedData> ExtractedData { get; set; } = new();
        public List<string> MissingInformation { get; set; } = new();
        public bool IsDataComplete { get; set; }
        public string NextQuestion { get; set; } = string.Empty;
        public string SuggestionTemplate { get; set; } = string.Empty;
        public string UserEngagementLevel { get; set; } = "medium";
        public bool ReadyToGenerate { get; set; }
        public bool RequiresDirectAnswer { get; set; }
        public string? DirectAnswer { get; set; }
    }
}
