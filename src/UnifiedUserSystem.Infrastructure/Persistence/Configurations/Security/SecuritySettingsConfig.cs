using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;

namespace UnifiedUserSystem.Infrastructure.Persistence.Configurations.Security;

public sealed class SecuritySettingsConfig : AuditableEntityConfig<SecuritySettings, Guid>
{
    public override void Configure(EntityTypeBuilder<SecuritySettings> builder)
    {
        base.Configure(builder);

        builder.ToTable("security_settings", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.IsMfaEnabled)
            .HasColumnName("is_mfa_enabled")
            .IsRequired();

        builder.Property(x => x.IsOtpEnabled)
            .HasColumnName("is_otp_enabled")
            .IsRequired();

        builder.Property(x => x.OtpExpirationMinutes)
            .HasColumnName("otp_expiration_minutes")
            .IsRequired();

        builder.Property(x => x.OtpMaxAttempts)
            .HasColumnName("otp_max_attempts")
            .IsRequired();

        builder.Property(x => x.LoginRateLimitPermitLimit)
            .HasColumnName("login_rate_limit_permit_limit")
            .IsRequired();

        builder.Property(x => x.LoginRateLimitWindowSeconds)
            .HasColumnName("login_rate_limit_window_seconds")
            .IsRequired();

        builder.Property(x => x.RefreshTokenRateLimitPermitLimit)
            .HasColumnName("refresh_token_rate_limit_permit_limit")
            .IsRequired();

        builder.Property(x => x.RefreshTokenRateLimitWindowSeconds)
            .HasColumnName("refresh_token_rate_limit_window_seconds")
            .IsRequired();

        builder.Property(x => x.AllowedIpRanges)
            .HasColumnName("allowed_ip_ranges")
            .HasMaxLength(SecuritySettings.IpRangesMaxLength)
            .IsRequired();

        builder.Property(x => x.BlockedIpRanges)
            .HasColumnName("blocked_ip_ranges")
            .HasMaxLength(SecuritySettings.IpRangesMaxLength)
            .IsRequired();

        builder.Property(x => x.LoginRateLimitQueueLimit)
            .HasColumnName("login_rate_limit_queue_limit")
            .IsRequired();

        builder.Property(x => x.LoginRateLimitCooldownSeconds)
            .HasColumnName("login_rate_limit_cooldown_seconds")
            .IsRequired();

        builder.Property(x => x.LoginLockoutFailureThreshold)
            .HasColumnName("login_lockout_failure_threshold")
            .IsRequired();

        builder.Property(x => x.LoginLockoutDurationSeconds)
            .HasColumnName("login_lockout_duration_seconds")
            .IsRequired();

        builder.Property(x => x.RefreshTokenRateLimitQueueLimit)
            .HasColumnName("refresh_token_rate_limit_queue_limit")
            .IsRequired();

        builder.Property(x => x.RefreshTokenRateLimitCooldownSeconds)
            .HasColumnName("refresh_token_rate_limit_cooldown_seconds")
            .IsRequired();

        builder.Property(x => x.SensitiveAdminRateLimitPermitLimit)
            .HasColumnName("sensitive_admin_rate_limit_permit_limit")
            .IsRequired();

        builder.Property(x => x.SensitiveAdminRateLimitWindowSeconds)
            .HasColumnName("sensitive_admin_rate_limit_window_seconds")
            .IsRequired();

        builder.Property(x => x.SensitiveAdminRateLimitQueueLimit)
            .HasColumnName("sensitive_admin_rate_limit_queue_limit")
            .IsRequired();

        builder.Property(x => x.SensitiveAdminRateLimitCooldownSeconds)
            .HasColumnName("sensitive_admin_rate_limit_cooldown_seconds")
            .IsRequired();
    }
}
