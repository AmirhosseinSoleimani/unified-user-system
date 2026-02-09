using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Catalog;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUser _currentUser;

        public CatalogService(IUnitOfWork uow, ICurrentUser currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<List<ProductListItemResponse>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            var products = await _uow.Products.GetActiveListAsync(cancellationToken);
            return products.Select(p => new ProductListItemResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Price,
                p.IsActive
                )).ToList();
        }

        public Task<ProductContentResponse> GetProductContentAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");
        }
    }
}
