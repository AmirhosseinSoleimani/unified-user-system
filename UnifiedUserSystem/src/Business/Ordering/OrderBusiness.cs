using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Order;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Business.Ordering
{
    public class OrderBusiness : IOrderBusiness
    {
        public void ValidateAddToCart(AddToCartRequest req)
        {
            if(req is null) throw new DomainException("Request in null.");

            Guard.True(req.ProductId != Guid.Empty, "ProductId is invalid.");
        }

        public void ValidateConfirm(ConfirmOrderRequest req)
        {
            if (req is null) throw new DomainException("Request in null.");

            Guard.True(req.OrderId != Guid.Empty, "OrderId is invalid.");
        }
    }
}
