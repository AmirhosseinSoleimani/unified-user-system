using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Catalog.Entities
{
    public class Product : AuditableEntity<Guid>
    {
        public const int TitleMaxLength = 250;
        public const int DescriptionMaxLength = 2000;

        public string Title { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public string Content { get; private set; } = default!;
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; } = true;
        public ICollection<ProductUser> ProductUsers { get; private set; } = new List<ProductUser>();

        private Product() { }

        public static Product Create(
            string title,
            string description,
            string content,
            decimal price,
            DateTimeOffset nowUtc,
            Guid? actorUser)
        {
            title = NormalizeTitle(title);
            description = NormalizeDescription(description);
            content = NormalizeContent(content);

            ValidateTitle(title);
            ValidateDescription(description);
            ValidateContent(content);

            Guard.True(price >= 0, "Price must be non-negative.");

            var product = new Product 
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                Content = content,
                Price = price,
                IsActive = true,
            };
            product.SetCreated(nowUtc, actorUser);
            return product;
        }

        public void Rename(string newTitle, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newTitle = NormalizeTitle(newTitle);
            ValidateTitle(newTitle);

            if (Title == newTitle) return;

            Title = newTitle;
            Touch(nowUtc, actorUserId);
        }

        public void UpdateDescription(string newDescription, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newDescription = NormalizeTitle(newDescription);
            ValidateDescription(newDescription);

            if (Description == newDescription) return;

            Description = newDescription;
            Touch(nowUtc, actorUserId);
        }

        public void UpdateContent(string newContent, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newContent = NormalizeContent(newContent);
            ValidateContent(newContent);

            if(Content == newContent) return;

            Content = newContent;
            Touch(nowUtc, actorUserId);
        }

        public void ChangePrice(decimal newPrice, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(newPrice >= 0, "Price must be non-negative");
            if (Price == newPrice) return;

            Price = newPrice;
            Touch(nowUtc, actorUserId);
        }

        public void Active(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (IsActive) return;

            IsActive = true;
            Touch(nowUtc, actorUserId);
        }

        public void Deactive(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!IsActive) return;

            IsActive = false;
            Touch(nowUtc, actorUserId);
        }

        public static string NormalizeTitle(string value) => (value ?? "").Trim();
        public static string NormalizeDescription(string value) => (value ?? "").Trim();
        public static string NormalizeContent(string value) => (value ?? "").Trim();
        private static void ValidateTitle(string value)
        {
            Guard.NotEmpty(value, nameof(Title));
            Guard.MaxLen(value, TitleMaxLength, nameof(Title));
        }
        public static void ValidateDescription(string value)
        {
            Guard.NotEmpty(value, nameof(Description));
            Guard.MaxLen(value, DescriptionMaxLength, nameof(Description));
        }
        public static void ValidateContent(string value)
        {
            Guard.NotEmpty(value, nameof(Content));
        }
    }
}
