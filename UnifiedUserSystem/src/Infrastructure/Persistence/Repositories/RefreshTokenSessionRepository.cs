using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenSessionRepository : IRefreshTokenSessionRepository
    {
        private readonly AppDbContext _dbContext;

        public RefreshTokenSessionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<RefreshTokenSession?> FindByHashAsync(string refreshTokenHash, CancellationToken ct = default)
        {
            return _dbContext.RefreshTokenSessions
                .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, ct);
        }

        public async Task<IReadOnlyList<RefreshTokenSession>> ListActiveByUserIdAsync(Guid userId, DateTimeOffset nowUtc, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokenSessions
                .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
                .OrderByDescending(x => x.IssuedAtUtc)
                .ToListAsync(ct);
        }

        public void Add(RefreshTokenSession session)
        {
            _dbContext.RefreshTokenSessions.Add(session);
        }
    }
}
