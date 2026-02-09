using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Product;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Business.Catalog
{
    public class ProductBusiness : IProductBusiness
    {
        public void ValidateCreate(CreateProductRequest req)
        {
            if (req is null) throw new DomainException("Request is null.");

            var title = Product.NormalizeTitle(Guard.NotEmpty(req.Title, nameof(req.Title)));
            var desc = Product.NormalizeDescription(Guard.NotEmpty(req.Description, nameof(req.Description)));
            var content = Product.NormalizeContent(Guard.NotEmpty(req.Content, nameof(req.Content)));

            Guard.MaxLen(title, Product.TitleMaxLength, nameof(req.Title));
            Guard.MaxLen(desc, Product.DescriptionMaxLength, nameof(req.Description));

            Guard.True(req.Price >= 0, "Price must be non-negative.");
        }

        public void ValidateUpdate(UpdateProductRequest req)
        {
            if (req is null) throw new DomainException("Request is null.");

            var title = Product.NormalizeTitle(Guard.NotEmpty(req.Title, nameof(req.Title)));
            var description = Product.NormalizeTitle(Guard.NotEmpty(req.Description, nameof(req.Description)));
            var content = Product.NormalizeTitle(Guard.NotEmpty(req.Content, nameof(req.Content)));

            Guard.MaxLen(title, Product.TitleMaxLength, nameof(req.Title));
            Guard.MaxLen(description, Product.DescriptionMaxLength, nameof(req.Description));

            Guard.True(req.Price >= 0, "Price must be non-negative");
        }
    }
}
