
namespace UnifiedUserSystem.src.Application.Options;

public sealed class SecuritySettingsDefaultsOptions
{
    public bool IsMfaEnabled { get; set; } = false;
    public bool IsOtpEnabled { get; set; } = true;
    public int OtpExpirationMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 5;
    public int LoginRateLimitPermitLimit { get; set; } = 10;
    public int LoginRateLimitWindowSeconds { get; set; } = 60;
    public int RefreshTokenRateLimitPermitLimit { get; set; } = 10;
    public int RefreshTokenRateLimitWindowSeconds { get; set; } = 60;
    public string[] AllowedIpRanges { get; set; } = Array.Empty<string>();
    public string[] BlockedIpRanges { get; set; } = Array.Empty<string>();
}
