using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations
{
    public class ProductUserConfig : AuditableEntityConfig<ProductUser, Guid>
    {
        public override void Configure(EntityTypeBuilder<ProductUser> builder)
        {
            base.Configure(builder);

            builder.ToTable("product_user", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();

            builder.HasOne<User>()
                .WithMany(u => u.ProductUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany(p => p.ProductUsers)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ProductId);
        }
    }
}
