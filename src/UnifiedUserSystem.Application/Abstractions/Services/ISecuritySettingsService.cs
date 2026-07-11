using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface ISecuritySettingsService
{
    Task<SecuritySettingsResponse> GetEffectiveAsync(CancellationToken ct = default);
    Task<SecuritySettingsResponse> UpdateAsync(UpdateSecuritySettingsRequest request, CancellationToken ct = default);
}
