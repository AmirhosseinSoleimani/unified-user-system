using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db, IUserRepository users) 
        {
            _db = db;
            Users = users;

        }
        public IUserRepository Users { get; }
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
