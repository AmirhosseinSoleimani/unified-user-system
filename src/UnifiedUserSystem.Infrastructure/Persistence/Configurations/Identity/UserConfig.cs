using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfig : AuditableEntityConfig<User, Guid>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.ToTable("users", "public");
            builder.HasKey(e => e.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(User.EmailMaxLength)
                .IsRequired();

            builder.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(User.UsernameMaxLength)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(User.FirstNameMaxLength)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(User.LastNameMaxLength)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(User.PhoneNumberMaxLength)
                .IsRequired(false);

            builder.Ignore(x => x.Fullname);

            builder.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(User.PasswordHashMaxLength)
                .IsRequired();

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.Username).IsUnique();
            
        }
    }
}
