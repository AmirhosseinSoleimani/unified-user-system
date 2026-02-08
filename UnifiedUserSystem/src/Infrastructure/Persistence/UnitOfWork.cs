using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(
            AppDbContext db,
            IUserRepository users,
            IRoleRepository roles,
            IOperationRepository operations,
            IRoleOperationRepository roleOperations,
            IProductRepository products,
            IOrderRepository orders,
            IProductUserRepository productUSers
            )
        {
            _db = db;
            Users = users;
            Roles = roles;
            Operations = operations;
            RoleOperations = roleOperations;
            Products = products;
            Orders = orders;
            ProductUsers = productUSers;
        }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IOperationRepository Operations { get; }
        public IRoleOperationRepository RoleOperations { get; }
        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public IProductUserRepository ProductUsers { get; }
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
