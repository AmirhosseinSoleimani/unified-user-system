using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.src.Application.Interfaces.Identity
{
    public interface IUserQueryService
    {
        Task<IReadOnlyList<ActiveUserListItemResponse>> ListActiveUsersAsync(CancellationToken ct = default);
    }
}
