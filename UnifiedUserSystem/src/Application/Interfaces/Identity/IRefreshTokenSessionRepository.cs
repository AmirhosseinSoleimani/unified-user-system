using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRefreshTokenSessionRepository
    {
        Task<RefreshTokenSession?> FindByHashAsync(string refreshTokenHash, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshTokenSession>> ListActiveByUserIdAsync(Guid userId, DateTimeOffset nowUtc, CancellationToken ct = default);
        void Add(RefreshTokenSession session);
    }
}
