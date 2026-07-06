using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly AppDbContext _db;
        public OperationRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Operation operation)
        {
            _db.Operation.Add(operation);
        }

        public async Task<IReadOnlyList<Operation>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Operation
                .AsNoTracking()
                .OrderBy(x => x.Key)
                .ToListAsync(ct);
        }

        public Task<Operation?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Operation.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<Operation?> FindByKeyAsync(string keyLower, CancellationToken ct = default)
        {
            return _db.Operation.FirstOrDefaultAsync(x => x.Key == keyLower, ct);
        }

        public Task<bool> HasAssignedRolesAsync(Guid operationId, CancellationToken ct = default)
        {
            return _db.RoleOperations.AnyAsync(x => x.OperationId == operationId, ct);
        }
    }
}
