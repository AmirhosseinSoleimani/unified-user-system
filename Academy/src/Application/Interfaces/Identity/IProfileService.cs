using UnifiedUserSystem.src.Contracts.DTOs.Profile;

namespace UnifiedUserSystem.src.Application.Interfaces.Identity
{
    public interface IProfileService
    {
        Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default);
    }
}
