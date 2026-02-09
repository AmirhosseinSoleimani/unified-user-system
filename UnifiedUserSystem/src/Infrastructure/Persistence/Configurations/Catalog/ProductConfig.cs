using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Catalog.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations
{
    public class ProductConfig : AuditableEntityConfig<Product, Guid>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            builder.ToTable("Products", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(Product.TitleMaxLength)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(Product.DescriptionMaxLength)
                .IsRequired();

            builder.Property(x => x.Content)
                .HasColumnName("content")
                .IsRequired();

            builder.Property(x => x.Price)
                .HasColumnName("price")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.Title);
        }
    }
}
