using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Domain.Ordering.Entities
{
    public class Order : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public bool IsConfirmed { get; private set; }
        public DateTimeOffset? ComfirmedAt { get; private set; }
        public User User { get; private set; } = default!;
        public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

        public Order() { }

        public static Order CreateForUser(Guid userId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");

            var order = new Order 
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsConfirmed = false,
                ComfirmedAt = null
            };

            order.SetCreated(nowUtc, actorUserId ?? userId);
            return order;
        }

        public void AddItem(Guid productId, decimal unitPrice, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(!IsConfirmed, "Cannot modify a confirmed order.");
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");
            Guard.True(unitPrice >= 0, "UnitPrice must be non-negative");

            if (Items.Any(x => x.ProductId == productId)) return;

            Items.Add(OrderItem.Create(Id, productId, unitPrice, nowUtc, actorUserId ?? UserId));
            Touch(nowUtc, actorUserId ?? UserId);
        }

        public void RemoveItem(Guid productId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(!IsConfirmed, "Cannot modify a confirm order.");
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");

            var item = Items.FirstOrDefault(x => x.ProductId == productId);
            if (item is  null) return;

            Items.Remove(item);
            Touch(nowUtc, actorUserId ?? UserId);
        }

        public decimal TotalPrice()
            => Items.Sum(x => x.UnitPrice);

        public void Confirm(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(!IsConfirmed, "Order already confirmed.");
            Guard.True(Items.Count > 0, "Order has no items.");

            IsConfirmed = true;
            ComfirmedAt = nowUtc;
            Touch(nowUtc, actorUserId ?? UserId);
        }

        public Guid[] GetProductIds()
            => Items.Select(x => x.ProductId).Distinct().ToArray();
    }
}
