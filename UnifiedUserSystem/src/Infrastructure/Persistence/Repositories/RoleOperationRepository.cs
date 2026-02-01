using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Repositories
{
    public class RoleOperationRepository : IRoleOperationRepository
    {
        private readonly AppDbContext _db;
        public RoleOperationRepository(AppDbContext db)
        {
            _db = db;
        }
        public Task AddAsync(RoleOperation roleOperation, CancellationToken cancellationToken = default)
        {
            _db.RoleOperations.Add(roleOperation);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(int roleId, Guid operationId)
        {
            return _db.RoleOperations.AnyAsync(x => x.RoleId == roleId && x.OperationId == operationId);
        }

        public Task<RoleOperation?> FindAsync(int roleId, Guid operationId, CancellationToken cancellationToken = default)
        {
            return _db.RoleOperations.FirstOrDefaultAsync(
                x => x.RoleId == roleId && x.OperationId == operationId,
                cancellationToken
                );
        }

        public void Remove(RoleOperation roleOperation)
        {
            _db.RoleOperations.Remove(roleOperation);
        }
    }
}
