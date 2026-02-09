using UnifiedUserSystem.src.Contracts.DTOs.Catalog;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface ICatalogService
    {
        Task<List<ProductListItemResponse>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductContentResponse> GetProductContentAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
