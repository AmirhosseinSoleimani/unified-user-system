using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(Product product)
        {
            _db.Products.Add(product);
        }

        public Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<Product>> GetActiveListAsync(CancellationToken cancellationToken = default)
        {
            return _db.Products.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
        }
    }
}
