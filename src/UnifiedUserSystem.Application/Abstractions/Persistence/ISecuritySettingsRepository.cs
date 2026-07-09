using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface ISecuritySettingsRepository
{
    Task<SecuritySettings?> GetAsync(CancellationToken ct = default);
    void Add(SecuritySettings settings);
}
