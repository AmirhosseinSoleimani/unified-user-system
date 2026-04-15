using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public class RoleConfig : AuditableEntityConfig<Role, int>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            builder.ToTable("roles", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(Role.NameMaxLength)
                .IsRequired();

            builder.Property(x => x.Key)
                .HasColumnName("key")
                .HasMaxLength(Role.KeyMaxLength)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Key).IsUnique();
        }

    }
}
