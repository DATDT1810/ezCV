using ezCV.Application.External.Models;
using ezCV.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.External
{
    public interface IGeminiAIService
    {
        Task<string> GenerateResponseAsync(List<ChatMessage> conversationHistory, long? userId = null);
        Task<AiAnalysisResult> AnalyzeUserResponseAsync(string userInput, ChatSession session);
        Task<CvSectionResult> GenerateCvSectionAsync(string section, ChatSession session);
    }
}
