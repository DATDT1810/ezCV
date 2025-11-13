using ezCV.Application.Repositories;
using ezCV.Domain.Entities;
using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Infrastructure.Repositories
{
    public class AIChatRepository : BaseRepository<ChatSession>, IAIChatRepository
    {
        private readonly ApplicationDbContext _context;
        public AIChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }

        public async Task<CvGenerationResult> AddGenerationResultAsync(CvGenerationResult result, CancellationToken cancellationToken = default)
        {
             _context.CvGenerationResults.Add(result);
            await _context.SaveChangesAsync();
            return result;

        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<ChatSession?> GetByGuidAsync(string sessionGuid, CancellationToken cancellationToken = default)
        {
            return await _context.ChatSessions
                .Include(cs => cs.ChatMessages.OrderBy(m => m.SentAt))
                .Include(cs => cs.CvGenerationResults)
                .Include(cs => cs.User)
                .FirstOrDefaultAsync(cs => cs.SessionGuid == sessionGuid, cancellationToken);
        }

        public async Task<List<CvGenerationResult>> GetGenerationResultsAsync(long sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.CvGenerationResults
                 .Where(cgr => cgr.SessionId == sessionId)
                 .OrderByDescending(cgr => cgr.GeneratedAt)
                 .ToListAsync(cancellationToken);
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(long sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .Where(cm => cm.SessionId == sessionId)
                .OrderBy(cm => cm.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(long userId, CancellationToken cancellationToken = default)
        {
           return await _context.ChatSessions
                .Where(cs => cs.UserId == userId)
                .Include(cs => cs.ChatMessages)
                  .OrderByDescending(cs => cs.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChatSession?> GetWithMessagesAsync(string sessionGuid, CancellationToken cancellationToken = default)
        {
            return await _context.ChatSessions
                .Include(cs => cs.ChatMessages.OrderBy(m => m.SentAt))
                .FirstOrDefaultAsync(cs => cs.SessionGuid == sessionGuid, cancellationToken);
        }
    }
}
