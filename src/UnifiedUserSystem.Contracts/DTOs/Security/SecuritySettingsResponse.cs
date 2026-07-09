
namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class SecuritySettingsResponse
{
    public Guid Id { get; set; }
    public bool IsMfaEnabled { get; set; }
    public bool IsOtpEnabled { get; set; }
    public int OtpExpirationMinutes { get; set; }
    public int OtpMaxAttempts { get; set; }
    public int LoginRateLimitPermitLimit { get; set; }
    public int LoginRateLimitWindowSeconds { get; set; }
    public int RefreshTokenRateLimitPermitLimit { get; set; }
    public int RefreshTokenRateLimitWindowSeconds { get; set; }
    public string[] AllowedIpRanges { get; set; } = Array.Empty<string>();
    public string[] BlockedIpRanges { get; set; } = Array.Empty<string>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
