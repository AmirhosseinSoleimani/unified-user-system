
namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class UpdateSecuritySettingsRequest
{
    public bool IsMfaEnabled { get; set; }
    public bool IsOtpEnabled { get; set; }
    public bool IsEmailOtpEnabled { get; set; } = true;
    public bool IsPhoneOtpEnabled { get; set; } = true;
    public int OtpExpirationMinutes { get; set; }
    public int OtpMaxAttempts { get; set; }
    public int LoginRateLimitPermitLimit { get; set; }
    public int LoginRateLimitWindowSeconds { get; set; }
    public int RefreshTokenRateLimitPermitLimit { get; set; }
    public int RefreshTokenRateLimitWindowSeconds { get; set; }
    public int LoginRateLimitQueueLimit { get; set; }
    public int LoginRateLimitCooldownSeconds { get; set; }
    public int LoginLockoutFailureThreshold { get; set; }
    public int LoginLockoutDurationSeconds { get; set; }
    public int RefreshTokenRateLimitQueueLimit { get; set; }
    public int RefreshTokenRateLimitCooldownSeconds { get; set; }
    public int SensitiveAdminRateLimitPermitLimit { get; set; }
    public int SensitiveAdminRateLimitWindowSeconds { get; set; }
    public int SensitiveAdminRateLimitQueueLimit { get; set; }
    public int SensitiveAdminRateLimitCooldownSeconds { get; set; }
    public string[] AllowedIpRanges { get; set; } = Array.Empty<string>();
    public string[] BlockedIpRanges { get; set; } = Array.Empty<string>();
}
