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
                .HasColumnType("id");

            builder.Property(x => x.Name)
                .HasColumnType("name")
                .HasMaxLength(Role.NameMaxLength)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.HasIndex(x => x.Name).IsUnique();
        }

    }
}
