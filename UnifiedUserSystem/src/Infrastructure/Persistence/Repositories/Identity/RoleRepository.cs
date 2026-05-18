using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _db;
        public RoleRepository(AppDbContext db) => _db = db;
        public void Add(Role role) => _db.Roles.Add(role);

        public Task<Role?> FindByIdAsync(int id, CancellationToken ct = default)
           => _db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Role?> FindByKeyAsync(string normalizeKey, CancellationToken ct = default)
            => _db.Roles.FirstOrDefaultAsync(x => x.Key == normalizeKey, ct);

        public Task<Role?> FindByNameAsync(string normalizedName, CancellationToken ct = default)
           => _db.Roles.FirstOrDefaultAsync(x => x.Name == normalizedName, ct);

        public Task<bool> ExistsByKeyAsync(string normalizeKey, CancellationToken ct = default) =>
            _db.Roles.AnyAsync(x => x.Key == normalizeKey, ct);

        public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default)
            => await _db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

        public Task<bool> HasAssignedUsersAsync(int roleId, CancellationToken ct = default) =>
            _db.UserRoles.AnyAsync(x => x.RoleId == roleId, ct);
    }
}
