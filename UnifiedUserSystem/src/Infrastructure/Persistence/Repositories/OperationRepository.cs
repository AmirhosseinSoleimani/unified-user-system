using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Repositories
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

        public Task<Operation?> FindByIdAsync(Guid id)
        {
            return _db.Operation.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Operation?> FindByKeyAsync(string keyLower)
        {
            return _db.Operation.FirstOrDefaultAsync(x => x.Key == keyLower);
        }
    }
}
