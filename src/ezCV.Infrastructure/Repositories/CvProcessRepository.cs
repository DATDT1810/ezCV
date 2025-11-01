using AutoMapper;
using ezCV.Application;
using ezCV.Application.Features.CvProcessing.Models;
using ezCV.Domain.Entities;
using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ezCV.Infrastructure.Repositories
{
    public class CvProcessRepository : ICvProcessRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CvProcessRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task SaveCvDataAsync(CvSubmissionRequest cvData, long userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lưu UserProfile
                var existingProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingProfile != null)
                {
                    // Update existing
                    _mapper.Map(cvData.Profile, existingProfile);
                    _context.UserProfiles.Update(existingProfile);
                }
                else
                {
                    // Create new
                    var profile = _mapper.Map<UserProfile>(cvData.Profile);
                    profile.UserId = userId;
                    await _context.UserProfiles.AddAsync(profile);
                }

                // 2. Lưu các sections
                await SaveSectionAsync(_context.WorkExperiences, cvData.WorkExperiences, userId);
                await SaveSectionAsync(_context.Educations, cvData.Educations, userId);
                await SaveSectionAsync(_context.Skills, cvData.Skills, userId);
                await SaveSectionAsync(_context.Projects, cvData.Projects, userId);
                await SaveSectionAsync(_context.Certificates, cvData.Certificates, userId);
                await SaveSectionAsync(_context.Awards, cvData.Awards, userId);
                await SaveSectionAsync(_context.References, cvData.References, userId);
                await SaveSectionAsync(_context.Languages, cvData.Languages, userId);
                await SaveSectionAsync(_context.Hobbies, cvData.Hobbies, userId);
                await SaveSectionAsync(_context.SocialLinks, cvData.SocialLinks, userId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SaveSectionAsync<TEntity, TRequest>(
            DbSet<TEntity> dbSet,
            List<TRequest> requests,
            long userId) where TEntity : class
        {
            // Xóa dữ liệu cũ
            var existing = dbSet.Where(e => EF.Property<long>(e, "UserId") == userId);
            dbSet.RemoveRange(existing);

            // Thêm dữ liệu mới
            if (requests?.Any() == true)
            {
                var entities = _mapper.Map<List<TEntity>>(requests);
                foreach (var entity in entities)
                {
                    _context.Entry(entity).Property("UserId").CurrentValue = userId;
                }
                await dbSet.AddRangeAsync(entities);
            }
        }

        public async Task<UserProfile> GetUserProfileAsync(long userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}