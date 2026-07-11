namespace UnifiedUserSystem.src.Application.Options;

public sealed class AuthProtectionOptions
{
    public int MaxFailedAttemptsPerIdentity { get; set; } = 5;
    public int MaxFailedAttemptsPerClient { get; set; } = 20;
    public int FailedAttemptWindowMinutes { get; set; } = 15;
    public int LockoutMinutes { get; set; } = 15;
    public int CooldownSeconds { get; set; } = 2;

    public int AuthRateLimitPermitLimit { get; set; } = 10;
    public int AuthRateLimitWindowSeconds { get; set; } = 60;
    public int AuthRateLimitQueueLimit { get; set; } = 0;

    public int SensitiveAdminRateLimitPermitLimit { get; set; } = 30;
    public int SensitiveAdminRateLimitWindowSeconds { get; set; } = 60;
    public int SensitiveAdminRateLimitQueueLimit { get; set; } = 0;
}