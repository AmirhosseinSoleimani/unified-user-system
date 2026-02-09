using UnifiedUserSystem.src.Domain.Ordering.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order?> FindOpenOrderForUSerAsync(Guid userId, CancellationToken cancellationToken = default);
        void Add(Order order);
    }
}
