using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IUserCommandService
{
    Task<ProfileResponse> UpdateUserAsync(Guid id, UpdateUserRequest req, CancellationToken ct = default);
    Task DeactivateUserAsync(Guid id, CancellationToken ct = default);

}
