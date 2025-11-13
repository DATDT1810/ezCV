using ezCV.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Repositories
{
    public interface IAIChatRepository : IBaseRepository<ChatSession>
    {
        Task<ChatSession?> GetByGuidAsync(string sessionGuid, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetWithMessagesAsync(string sessionGuid, CancellationToken cancellationToken = default);
        Task<ChatMessage> AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task<CvGenerationResult> AddGenerationResultAsync(CvGenerationResult result, CancellationToken cancellationToken = default);
        Task<List<ChatSession>> GetUserSessionsAsync(long userId, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetSessionMessagesAsync(long sessionId, CancellationToken cancellationToken = default);
        Task<List<CvGenerationResult>> GetGenerationResultsAsync(long sessionId, CancellationToken cancellationToken = default);
    }
}
