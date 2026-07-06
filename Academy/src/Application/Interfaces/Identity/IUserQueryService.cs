using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.src.Application.Interfaces.Identity
{
    public interface IUserQueryService
    {
        Task<IReadOnlyList<ActiveUserListItemResponse>> ListActiveUsersAsync(CancellationToken ct = default);
        Task<ProfileResponse> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    }
}
