using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(string name, CancellationToken ct = default);
        Task RenameRoleAsync(int roleId, string newName, CancellationToken ct = default);
        Task ActivateRoleAsync(int roleId, CancellationToken ct = default);
        Task DeactivateRoleAsync(int roleId, CancellationToken ct = default);
        Task AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default);
        Task RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default);
    }
}
