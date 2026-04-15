using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> FindByIdAsync(int id, CancellationToken ct = default);
        Task<Role?> FindByNameAsync(string normalizeName, CancellationToken ct = default);
        Task<Role?> FindByKeyAsync(string normalizeKey, CancellationToken ct = default);
        Task<bool> ExistsByKeyAsync(string normalizeKey, CancellationToken ct = default);
        void Add(Role role);
    }
}
