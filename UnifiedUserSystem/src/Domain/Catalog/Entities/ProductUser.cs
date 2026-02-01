using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Domain.Catalog.Entities
{
    public class ProductUser : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public Guid ProductId { get; private set; }
        public User User { get; private set; } = default!;
        public Product Product { get; private set; } = default!;

        private ProductUser() { }

        public static ProductUser Grant(
            Guid userId,
            Guid productId,
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");
            Guard.True(productId != Guid.Empty, "ProductId is invalid.");

            var productUser = new ProductUser
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
            };

            productUser.SetCreated(nowUtc, actorUserId ?? userId);
            return productUser;
        }
    }
}
