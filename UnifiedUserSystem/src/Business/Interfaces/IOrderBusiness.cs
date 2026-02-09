using UnifiedUserSystem.src.Contracts.DTOs.Order;

namespace UnifiedUserSystem.src.Business.Interfaces
{
    public interface IOrderBusiness
    {
        void ValidateAddToCart(AddToCartRequest req);
        void ValidateConfirm(ConfirmOrderRequest req);
    }
}
