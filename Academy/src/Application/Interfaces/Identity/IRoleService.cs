using UnifiedUserSystem.src.Contracts.DTOs.Roles;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
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
        Task<UserRolesResponse> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
        Task<UserRolesResponse> AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default);
        Task<UserRolesResponse> RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default);
        Task<UserRolesResponse> ReplaceUserRolesAsync(Guid userId, IReadOnlyCollection<int> roleIds, CancellationToken ct = default);
        Task<RoleOperationsResponse> GetRoleOperationsAsync(int roleId, CancellationToken ct = default);
        Task<RoleOperationsResponse> AssignOperationToRoleAsync(int roleId, Guid operationId, CancellationToken ct = default);
        Task<RoleOperationsResponse> RemoveOperationFromRoleAsync(int roleId, Guid operationId, CancellationToken ct = default);
        Task<RoleOperationsResponse> ReplaceRoleOperationsAsync(int roleId, IReadOnlyCollection<Guid> operationIds, CancellationToken ct = default);
    }
}
