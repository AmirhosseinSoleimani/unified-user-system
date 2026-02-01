using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Ordering.Entities
{
    public class OrderItem : AuditableEntity<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public decimal UnitPrice { get; private set; }
        public Order Order { get; private set; } = default!;
        public Product Product { get; private set; } = default!;

        private OrderItem() { }

        public static OrderItem Create(Guid orderId, Guid productId, decimal unitPrice, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(orderId != Guid.Empty, "OrderId is invalid");
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");
            Guard.True(unitPrice >= 0, "UnitPrice must be non-negative.");

            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                UnitPrice = unitPrice,
            };

            item.SetCreated(nowUtc, actorUserId);

            return item;
        }

        public void ChangePrice(decimal newPrice, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(newPrice >= 0, "UnitPrice must be non-negative");
            if (UnitPrice == newPrice) return;

            UnitPrice = newPrice;
            Touch(nowUtc, actorUserId);
        }
    }
}
