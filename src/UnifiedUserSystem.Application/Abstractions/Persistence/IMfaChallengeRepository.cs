
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IMfaChallengeRepository
{
    Task<MfaChallenge?> FindByIdAsync(Guid id, CancellationToken ct = default);
    void Add(MfaChallenge challenge);
}
