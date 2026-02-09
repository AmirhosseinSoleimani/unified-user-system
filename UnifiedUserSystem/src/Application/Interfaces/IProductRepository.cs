using UnifiedUserSystem.src.Domain.Catalog.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Product>> GetActiveListAsync(CancellationToken cancellationToken = default);
        void Add(Product product);

    }
}
