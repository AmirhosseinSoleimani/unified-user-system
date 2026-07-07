using UnifiedUserSystem.src.Contracts.DTOs.Profile;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IProfileService
{
    Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default);
}
