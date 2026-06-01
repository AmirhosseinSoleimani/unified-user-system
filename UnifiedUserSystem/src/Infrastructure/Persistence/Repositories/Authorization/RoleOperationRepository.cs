using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class RoleOperationRepository : IRoleOperationRepository
    {
        private readonly AppDbContext _db;
        public RoleOperationRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IReadOnlyList<RoleOperation>> ListByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
        {
            return await _db.RoleOperations
                .Include(x => x.Operation)
                .Where(x => x.RoleId == roleId)
                .OrderBy(x => x.Operation.Key)
                .ToListAsync(cancellationToken);
        }

        public Task AddAsync(RoleOperation roleOperation, CancellationToken cancellationToken = default)
        {
            _db.RoleOperations.Add(roleOperation);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(int roleId, Guid operationId, CancellationToken cancellationToken = default)
        {
            return _db.RoleOperations.AnyAsync(
                x => x.RoleId == roleId && x.OperationId == operationId,
                cancellationToken);
        }

        public Task<RoleOperation?> FindAsync(int roleId, Guid operationId, CancellationToken cancellationToken = default)
        {
            return _db.RoleOperations
                .Include(x => x.Operation)
                .FirstOrDefaultAsync(
                    x => x.RoleId == roleId && x.OperationId == operationId,
                    cancellationToken);
        }

        public void Remove(RoleOperation roleOperation)
        {
            _db.RoleOperations.Remove(roleOperation);
        }
    }
}
