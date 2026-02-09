using UnifiedUserSystem.src.Contracts.DTOs.Product;

namespace UnifiedUserSystem.src.Business.Interfaces
{
    public interface IProductBusiness
    {
        void ValidateCreate(CreateProductRequest req);
        void ValidateUpdate(UpdateProductRequest req);
    }
}
