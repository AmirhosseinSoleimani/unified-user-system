using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IReadOnlyList<Role>> ListRolesAsync(CancellationToken ct = default);
        Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct = default);
        Task<Role> CreateRoleAsync(string name, CancellationToken ct = default);
        Task<Role> UpdateRoleAsync(int roleId, string name, CancellationToken ct = default);
        Task DeleteRoleAsync(int roleId, CancellationToken ct = default);
        Task RenameRoleAsync(int roleId, string newName, CancellationToken ct = default);
        Task ActivateRoleAsync(int roleId, CancellationToken ct = default);
        Task DeactivateRoleAsync(int roleId, CancellationToken ct = default);
        Task AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default);
        Task RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default);
    }
}
