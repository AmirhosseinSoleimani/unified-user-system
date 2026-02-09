using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Domain.Ordering.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations
{
    public class OrderConfig : AuditableEntityConfig<Order, Guid>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);

            builder.ToTable("orders", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.IsConfirmed)
                .HasColumnName("is_confirmed")
                .IsRequired();

            builder.Property(x => x.ComfirmedAt)
                .HasColumnName("confirmed_at")
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.IsConfirmed);
            builder.HasIndex(X => X.ComfirmedAt);
        }
    }
}
