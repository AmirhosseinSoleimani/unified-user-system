using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;

public sealed class MfaChallengeRepository : IMfaChallengeRepository
{
    private readonly AppDbContext _db;

    public MfaChallengeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<MfaChallenge?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.MfaChallenges
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public void Add(MfaChallenge challenge)
    {
        _db.MfaChallenges.Add(challenge);
    }
}
