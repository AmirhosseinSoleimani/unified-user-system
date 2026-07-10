
namespace UnifiedUserSystem.src.Application.Options;

public sealed class SecuritySettingsDefaultsOptions
{
    public bool IsMfaEnabled { get; set; } = false;
    public bool IsOtpEnabled { get; set; } = true;
    public bool IsEmailOtpEnabled { get; set; } = true;
    public bool IsPhoneOtpEnabled { get; set; } = true;
    public int OtpExpirationMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 5;
    public int LoginRateLimitPermitLimit { get; set; } = 10;
    public int LoginRateLimitWindowSeconds { get; set; } = 60;
    public int RefreshTokenRateLimitPermitLimit { get; set; } = 10;
    public int RefreshTokenRateLimitWindowSeconds { get; set; } = 60;
    public int LoginRateLimitQueueLimit { get; set; } = 0;
    public int LoginRateLimitCooldownSeconds { get; set; } = 2;
    public int LoginLockoutFailureThreshold { get; set; } = 5;
    public int LoginLockoutDurationSeconds { get; set; } = 900;
    public int RefreshTokenRateLimitQueueLimit { get; set; } = 0;
    public int RefreshTokenRateLimitCooldownSeconds { get; set; } = 2;
    public int SensitiveAdminRateLimitPermitLimit { get; set; } = 30;
    public int SensitiveAdminRateLimitWindowSeconds { get; set; } = 60;
    public int SensitiveAdminRateLimitQueueLimit { get; set; } = 0;
    public int SensitiveAdminRateLimitCooldownSeconds { get; set; } = 2;
    public string[] AllowedIpRanges { get; set; } = Array.Empty<string>();
    public string[] BlockedIpRanges { get; set; } = Array.Empty<string>();
}
