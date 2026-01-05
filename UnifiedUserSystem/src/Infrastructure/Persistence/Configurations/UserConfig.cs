using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfig : AuditableEntityConfig<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "public");

            builder.HasKey(e => e.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

            builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

            builder.Property(x => x.Fullname)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

            builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(72)
            .IsRequired();

            builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("user");

            base.Configure(builder);
        }
    }
}
