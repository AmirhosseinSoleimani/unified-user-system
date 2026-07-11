
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IIpRuleRepository
{
    Task<IpRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<IpRule>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<IpRule>> ListActiveAsync(CancellationToken ct = default);
    void Add(IpRule rule);
}
