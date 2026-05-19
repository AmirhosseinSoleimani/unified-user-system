using UnifiedUserSystem.src.Contracts.DTOs.Roles;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default);
        Task<Role?> FindByIdAsync(int id, CancellationToken ct = default);
        Task<Role?> FindByNameAsync(string normalizeName, CancellationToken ct = default);
        Task<Role?> FindByKeyAsync(string normalizeKey, CancellationToken ct = default);
        Task<bool> ExistsByKeyAsync(string normalizeKey, CancellationToken ct = default);
        Task<bool> HasAssignedUsersAsync(int roleId, CancellationToken ct = default);
        void Add(Role role);
    }
}
