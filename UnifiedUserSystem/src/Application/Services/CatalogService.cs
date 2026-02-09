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

        public async Task<ProductContentResponse> GetProductContentAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");

            var userId = _currentUser.UserId;
            Guard.True(userId is not null, "User is not authenticated.");

            var hasAccess = await _uow.ProductUsers.HasAccessAsync(userId.Value, productId, cancellationToken);
            if (!hasAccess)
                throw new DomainException("You don't have access to this product content.");

            var product = await _uow.Products.FindByIdAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is not active.");

            return new ProductContentResponse(product.Id, product.Title, product.Content);

        }
    }
}
