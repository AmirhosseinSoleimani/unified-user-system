using SecuritySettingsEntity =
    UnifiedUserSystem.src.Domain.Security.Entities.SecuritySettings;

namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

public sealed class SecuritySettingsTestData
{
    internal static readonly DateTimeOffset DefaultNow =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid DefaultActorId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal bool IsMfaEnabled { get; set; } = true;
    internal bool IsOtpEnabled { get; set; } = true;
    internal bool IsEmailOtpEnabled { get; set; } = true;
    internal bool IsPhoneOtpEnabled { get; set; } = true;

    internal int OtpExpirationMinutes { get; set; } = 5;
    internal int OtpMaxAttempts { get; set; } = 5;

    internal int LoginRateLimitPermitLimit { get; set; } = 10;
    internal int LoginRateLimitWindowSeconds { get; set; } = 60;
    internal int LoginRateLimitQueueLimit { get; set; } = 0;
    internal int LoginRateLimitCooldownSeconds { get; set; } = 2;
    internal int LoginLockoutFailureThreshold { get; set; } = 5;
    internal int LoginLockoutDurationSeconds { get; set; } = 900;

    internal int RefreshTokenRateLimitPermitLimit { get; set; } = 10;
    internal int RefreshTokenRateLimitWindowSeconds { get; set; } = 60;
    internal int RefreshTokenRateLimitQueueLimit { get; set; } = 0;
    internal int RefreshTokenRateLimitCooldownSeconds { get; set; } = 2;

    internal int SensitiveAdminRateLimitPermitLimit { get; set; } = 30;
    internal int SensitiveAdminRateLimitWindowSeconds { get; set; } = 60;
    internal int SensitiveAdminRateLimitQueueLimit { get; set; } = 0;
    internal int SensitiveAdminRateLimitCooldownSeconds { get; set; } = 2;

    internal IReadOnlyCollection<string>? AllowedIpRanges { get; set; } =
        new[] { "10.0.0.0/8", "192.168.1.10" };

    internal IReadOnlyCollection<string>? BlockedIpRanges { get; set; } =
        new[] { "203.0.113.0/24", "2001:db8::/32" };

    internal DateTimeOffset NowUtc { get; set; } = DefaultNow;

    internal Guid? ActorUserId { get; set; } = DefaultActorId;

    internal SecuritySettingsEntity Create()
    {
        return SecuritySettingsEntity.Create(
            isMfaEnabled: IsMfaEnabled,
            isOtpEnabled: IsOtpEnabled,
            isEmailOtpEnabled: IsEmailOtpEnabled,
            isPhoneOtpEnabled: IsPhoneOtpEnabled,
            otpExpirationMinutes: OtpExpirationMinutes,
            otpMaxAttempts: OtpMaxAttempts,
            loginRateLimitPermitLimit: LoginRateLimitPermitLimit,
            loginRateLimitWindowSeconds: LoginRateLimitWindowSeconds,
            loginRateLimitQueueLimit: LoginRateLimitQueueLimit,
            loginRateLimitCooldownSeconds: LoginRateLimitCooldownSeconds,
            loginLockoutFailureThreshold: LoginLockoutFailureThreshold,
            loginLockoutDurationSeconds: LoginLockoutDurationSeconds,
            refreshTokenRateLimitPermitLimit: RefreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds: RefreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit: RefreshTokenRateLimitQueueLimit,
            refreshTokenRateLimitCooldownSeconds: RefreshTokenRateLimitCooldownSeconds,
            sensitiveAdminRateLimitPermitLimit:
                SensitiveAdminRateLimitPermitLimit,
            sensitiveAdminRateLimitWindowSeconds:
                SensitiveAdminRateLimitWindowSeconds,
            sensitiveAdminRateLimitQueueLimit:
                SensitiveAdminRateLimitQueueLimit,
            sensitiveAdminRateLimitCooldownSeconds:
                SensitiveAdminRateLimitCooldownSeconds,
            allowedIpRanges: AllowedIpRanges,
            blockedIpRanges: BlockedIpRanges,
            nowUtc: NowUtc,
            actorUserId: ActorUserId);
    }

    internal void Update(
        UnifiedUserSystem.src.Domain.Security.Entities.SecuritySettings settings)
    {
        settings.Update(
            isMfaEnabled: IsMfaEnabled,
            isOtpEnabled: IsOtpEnabled,
            isEmailOtpEnabled: IsEmailOtpEnabled,
            isPhoneOtpEnabled: IsPhoneOtpEnabled,
            otpExpirationMinutes: OtpExpirationMinutes,
            otpMaxAttempts: OtpMaxAttempts,
            loginRateLimitPermitLimit: LoginRateLimitPermitLimit,
            loginRateLimitWindowSeconds: LoginRateLimitWindowSeconds,
            loginRateLimitQueueLimit: LoginRateLimitQueueLimit,
            loginRateLimitCooldownSeconds: LoginRateLimitCooldownSeconds,
            loginLockoutFailureThreshold: LoginLockoutFailureThreshold,
            loginLockoutDurationSeconds: LoginLockoutDurationSeconds,
            refreshTokenRateLimitPermitLimit: RefreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds: RefreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit: RefreshTokenRateLimitQueueLimit,
            refreshTokenRateLimitCooldownSeconds: RefreshTokenRateLimitCooldownSeconds,
            sensitiveAdminRateLimitPermitLimit:
                SensitiveAdminRateLimitPermitLimit,
            sensitiveAdminRateLimitWindowSeconds:
                SensitiveAdminRateLimitWindowSeconds,
            sensitiveAdminRateLimitQueueLimit:
                SensitiveAdminRateLimitQueueLimit,
            sensitiveAdminRateLimitCooldownSeconds:
                SensitiveAdminRateLimitCooldownSeconds,
            allowedIpRanges: AllowedIpRanges,
            blockedIpRanges: BlockedIpRanges,
            nowUtc: NowUtc,
            actorUserId: ActorUserId);
    }

    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}
