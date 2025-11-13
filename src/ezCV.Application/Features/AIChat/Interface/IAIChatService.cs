using ezCV.Application.Features.AIChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat.Interface
{
    public interface IAIChatService
    {
        Task<ChatResponse> StartCvCreationAsync(long? userId);
        Task<ChatResponse> SendMessageAsync(ChatRequest request, long? userId = null);
        Task<CvGenerationResult> GenerateCvSectionAsync(string sessionGuid, string section);
        Task<List<ChatSession>> GetUserSessionsAsync(long userId);
        Task<ChatSession?> GetSessionAsync(string sessionGuid);
    }
}
