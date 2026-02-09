using UnifiedUserSystem.src.Contracts.DTOs.Order;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOrderService
    {
        Task<CartResponse> GetMyOpenCartAsync(CancellationToken cancellationToken = default);

        Task<CartResponse> AddToMyCartAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<CartResponse> ConfirmMyCartAsync(CancellationToken cancellationToken = default);
    }
}
