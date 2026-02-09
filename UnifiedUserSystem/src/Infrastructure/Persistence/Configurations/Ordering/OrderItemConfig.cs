using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Domain.Ordering.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfig : AuditableEntityConfig<OrderItem, Guid>
    {
        public override void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            base.Configure(builder);

            builder.ToTable("order_items", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne<Order>()
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.Product)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new {x.OrderId, x.ProductId}).IsUnique();

            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(X => X.ProductId);
        }
    }
}
