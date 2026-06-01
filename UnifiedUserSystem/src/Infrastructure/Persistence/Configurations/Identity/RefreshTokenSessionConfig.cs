using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Identity
{
    public sealed class RefreshTokenSessionConfig : AuditableEntityConfig<RefreshTokenSession, Guid>
    {
        public override void Configure(EntityTypeBuilder<RefreshTokenSession> builder)
        {
            base.Configure(builder);

            builder.ToTable("refresh_token_sessions", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.RefreshTokenHash)
                .HasColumnName("refresh_token_hash")
                .HasMaxLength(RefreshTokenSession.RefreshTokenHashMaxLength)
                .IsRequired();

            builder.Property(x => x.IssuedAtUtc)
                .HasColumnName("issued_at_utc")
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .IsRequired();

            builder.Property(x => x.RevokedAtUtc)
                .HasColumnName("revoked_at_utc")
                .IsRequired(false);

            builder.Property(x => x.ReplacedBySessionId)
                .HasColumnName("replaced_by_session_id")
                .IsRequired(false);

            builder.Property(x => x.ReuseDetectedAtUtc)
                .HasColumnName("reuse_detected_at_utc")
                .IsRequired(false);

            builder.Property(x => x.DeviceName)
                .HasColumnName("device_name")
                .HasMaxLength(RefreshTokenSession.DeviceNameMaxLength)
                .IsRequired(false);

            builder.Property(x => x.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(RefreshTokenSession.UserAgentMaxLength)
                .IsRequired(false);

            builder.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(RefreshTokenSession.IpAddressMaxLength)
                .IsRequired(false);

            builder.Property(x => x.ClientId)
                .HasColumnName("client_id")
                .HasMaxLength(RefreshTokenSession.ClientIdMaxLength)
                .IsRequired(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokenSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ExpiresAtUtc);
        }
    }
}
