using System.Net;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Security.Entities;

public sealed class SecuritySettings : AuditableEntity<Guid>
{
    public const int IpRangesMaxLength = 4000;
    public static readonly Guid SingletonId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public bool IsMfaEnabled { get; private set; }
    public bool IsOtpEnabled { get; private set; }
    public bool IsEmailOtpEnabled { get; private set; }
    public bool IsPhoneOtpEnabled { get; private set; }
    public int OtpExpirationMinutes { get; private set; }
    public int OtpMaxAttempts { get; private set; }

    public int LoginRateLimitPermitLimit { get; private set; }
    public int LoginRateLimitWindowSeconds { get; private set; }
    public int LoginRateLimitQueueLimit { get; private set; }
    public int LoginRateLimitCooldownSeconds { get; private set; }
    public int LoginLockoutFailureThreshold { get; private set; }
    public int LoginLockoutDurationSeconds { get; private set; }

    public int RefreshTokenRateLimitPermitLimit { get; private set; }
    public int RefreshTokenRateLimitWindowSeconds { get; private set; }
    public int RefreshTokenRateLimitQueueLimit { get; private set; }
    public int RefreshTokenRateLimitCooldownSeconds { get; private set; }

    public int SensitiveAdminRateLimitPermitLimit { get; private set; }
    public int SensitiveAdminRateLimitWindowSeconds { get; private set; }
    public int SensitiveAdminRateLimitQueueLimit { get; private set; }
    public int SensitiveAdminRateLimitCooldownSeconds { get; private set; }

    public string AllowedIpRanges { get; private set; } = string.Empty;
    public string BlockedIpRanges { get; private set; } = string.Empty;

    private SecuritySettings()
    {
    }

    public static SecuritySettings CreateDefault(DateTimeOffset nowUtc, Guid? actorUserId)
    {
        return Create(
            isMfaEnabled: false,
            isOtpEnabled: true,
            isEmailOtpEnabled: true,
            isPhoneOtpEnabled: true,
            otpExpirationMinutes: 5,
            otpMaxAttempts: 5,
            loginRateLimitPermitLimit: 10,
            loginRateLimitWindowSeconds: 60,
            loginRateLimitQueueLimit: 0,
            loginRateLimitCooldownSeconds: 2,
            loginLockoutFailureThreshold: 5,
            loginLockoutDurationSeconds: 900,
            refreshTokenRateLimitPermitLimit: 10,
            refreshTokenRateLimitWindowSeconds: 60,
            refreshTokenRateLimitQueueLimit: 0,
            refreshTokenRateLimitCooldownSeconds: 2,
            sensitiveAdminRateLimitPermitLimit: 30,
            sensitiveAdminRateLimitWindowSeconds: 60,
            sensitiveAdminRateLimitQueueLimit: 0,
            sensitiveAdminRateLimitCooldownSeconds: 2,
            allowedIpRanges: Array.Empty<string>(),
            blockedIpRanges: Array.Empty<string>(),
            nowUtc: nowUtc,
            actorUserId: actorUserId);
    }

    public static SecuritySettings Create(
        bool isMfaEnabled,
        bool isOtpEnabled,
        bool isEmailOtpEnabled,
        bool isPhoneOtpEnabled,
        int otpExpirationMinutes,
        int otpMaxAttempts,
        int loginRateLimitPermitLimit,
        int loginRateLimitWindowSeconds,
        int loginRateLimitQueueLimit,
        int loginRateLimitCooldownSeconds,
        int loginLockoutFailureThreshold,
        int loginLockoutDurationSeconds,
        int refreshTokenRateLimitPermitLimit,
        int refreshTokenRateLimitWindowSeconds,
        int refreshTokenRateLimitQueueLimit,
        int refreshTokenRateLimitCooldownSeconds,
        int sensitiveAdminRateLimitPermitLimit,
        int sensitiveAdminRateLimitWindowSeconds,
        int sensitiveAdminRateLimitQueueLimit,
        int sensitiveAdminRateLimitCooldownSeconds,
        IReadOnlyCollection<string>? allowedIpRanges,
        IReadOnlyCollection<string>? blockedIpRanges,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        var settings = new SecuritySettings
        {
            Id = SingletonId
        };

        settings.Apply(
            isMfaEnabled,
            isOtpEnabled,
            isEmailOtpEnabled,
            isPhoneOtpEnabled,
            otpExpirationMinutes,
            otpMaxAttempts,
            loginRateLimitPermitLimit,
            loginRateLimitWindowSeconds,
            loginRateLimitQueueLimit,
            loginRateLimitCooldownSeconds,
            loginLockoutFailureThreshold,
            loginLockoutDurationSeconds,
            refreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit,
            refreshTokenRateLimitCooldownSeconds,
            sensitiveAdminRateLimitPermitLimit,
            sensitiveAdminRateLimitWindowSeconds,
            sensitiveAdminRateLimitQueueLimit,
            sensitiveAdminRateLimitCooldownSeconds,
            allowedIpRanges,
            blockedIpRanges);

        settings.SetCreated(nowUtc, actorUserId);
        return settings;
    }

    public void Update(
        bool isMfaEnabled,
        bool isOtpEnabled,
        bool isEmailOtpEnabled,
        bool isPhoneOtpEnabled,
        int otpExpirationMinutes,
        int otpMaxAttempts,
        int loginRateLimitPermitLimit,
        int loginRateLimitWindowSeconds,
        int loginRateLimitQueueLimit,
        int loginRateLimitCooldownSeconds,
        int loginLockoutFailureThreshold,
        int loginLockoutDurationSeconds,
        int refreshTokenRateLimitPermitLimit,
        int refreshTokenRateLimitWindowSeconds,
        int refreshTokenRateLimitQueueLimit,
        int refreshTokenRateLimitCooldownSeconds,
        int sensitiveAdminRateLimitPermitLimit,
        int sensitiveAdminRateLimitWindowSeconds,
        int sensitiveAdminRateLimitQueueLimit,
        int sensitiveAdminRateLimitCooldownSeconds,
        IReadOnlyCollection<string>? allowedIpRanges,
        IReadOnlyCollection<string>? blockedIpRanges,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        Apply(
            isMfaEnabled,
            isOtpEnabled,
            isEmailOtpEnabled,
            isPhoneOtpEnabled,
            otpExpirationMinutes,
            otpMaxAttempts,
            loginRateLimitPermitLimit,
            loginRateLimitWindowSeconds,
            loginRateLimitQueueLimit,
            loginRateLimitCooldownSeconds,
            loginLockoutFailureThreshold,
            loginLockoutDurationSeconds,
            refreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit,
            refreshTokenRateLimitCooldownSeconds,
            sensitiveAdminRateLimitPermitLimit,
            sensitiveAdminRateLimitWindowSeconds,
            sensitiveAdminRateLimitQueueLimit,
            sensitiveAdminRateLimitCooldownSeconds,
            allowedIpRanges,
            blockedIpRanges);

        Touch(nowUtc, actorUserId);
    }

    public string[] GetAllowedIpRanges() => SplitRanges(AllowedIpRanges);

    public string[] GetBlockedIpRanges() => SplitRanges(BlockedIpRanges);

    private void Apply(
        bool isMfaEnabled,
        bool isOtpEnabled,
        bool isEmailOtpEnabled,
        bool isPhoneOtpEnabled,
        int otpExpirationMinutes,
        int otpMaxAttempts,
        int loginRateLimitPermitLimit,
        int loginRateLimitWindowSeconds,
        int loginRateLimitQueueLimit,
        int loginRateLimitCooldownSeconds,
        int loginLockoutFailureThreshold,
        int loginLockoutDurationSeconds,
        int refreshTokenRateLimitPermitLimit,
        int refreshTokenRateLimitWindowSeconds,
        int refreshTokenRateLimitQueueLimit,
        int refreshTokenRateLimitCooldownSeconds,
        int sensitiveAdminRateLimitPermitLimit,
        int sensitiveAdminRateLimitWindowSeconds,
        int sensitiveAdminRateLimitQueueLimit,
        int sensitiveAdminRateLimitCooldownSeconds,
        IReadOnlyCollection<string>? allowedIpRanges,
        IReadOnlyCollection<string>? blockedIpRanges)
    {
        Guard.True(otpExpirationMinutes > 0, "OtpExpirationMinutes must be greater than 0.");
        Guard.True(otpMaxAttempts > 0, "OtpMaxAttempts must be greater than 0.");

        Guard.True(loginRateLimitPermitLimit > 0, "LoginRateLimitPermitLimit must be greater than 0.");
        Guard.True(loginRateLimitWindowSeconds > 0, "LoginRateLimitWindowSeconds must be greater than 0.");
        Guard.True(loginRateLimitQueueLimit >= 0, "LoginRateLimitQueueLimit must be greater than or equal to 0.");
        Guard.True(loginRateLimitCooldownSeconds >= 0, "LoginRateLimitCooldownSeconds must be greater than or equal to 0.");
        Guard.True(loginLockoutFailureThreshold > 0, "LoginLockoutFailureThreshold must be greater than 0.");
        Guard.True(loginLockoutDurationSeconds > 0, "LoginLockoutDurationSeconds must be greater than 0.");

        Guard.True(refreshTokenRateLimitPermitLimit > 0, "RefreshTokenRateLimitPermitLimit must be greater than 0.");
        Guard.True(refreshTokenRateLimitWindowSeconds > 0, "RefreshTokenRateLimitWindowSeconds must be greater than 0.");
        Guard.True(refreshTokenRateLimitQueueLimit >= 0, "RefreshTokenRateLimitQueueLimit must be greater than or equal to 0.");
        Guard.True(refreshTokenRateLimitCooldownSeconds >= 0, "RefreshTokenRateLimitCooldownSeconds must be greater than or equal to 0.");

        Guard.True(sensitiveAdminRateLimitPermitLimit > 0, "SensitiveAdminRateLimitPermitLimit must be greater than 0.");
        Guard.True(sensitiveAdminRateLimitWindowSeconds > 0, "SensitiveAdminRateLimitWindowSeconds must be greater than 0.");
        Guard.True(sensitiveAdminRateLimitQueueLimit >= 0, "SensitiveAdminRateLimitQueueLimit must be greater than or equal to 0.");
        Guard.True(sensitiveAdminRateLimitCooldownSeconds >= 0, "SensitiveAdminRateLimitCooldownSeconds must be greater than or equal to 0.");

        var normalizedAllowed = NormalizeRanges(allowedIpRanges);
        var normalizedBlocked = NormalizeRanges(blockedIpRanges);

        var conflicts = normalizedAllowed.Intersect(normalizedBlocked, StringComparer.OrdinalIgnoreCase).ToArray();
        if (conflicts.Length > 0)
            throw new DomainException("AllowedIpRanges and BlockedIpRanges must not contain the same value.");

        var allowed = JoinRanges(normalizedAllowed);
        var blocked = JoinRanges(normalizedBlocked);

        Guard.MaxLen(allowed, IpRangesMaxLength, nameof(AllowedIpRanges));
        Guard.MaxLen(blocked, IpRangesMaxLength, nameof(BlockedIpRanges));

        IsMfaEnabled = isMfaEnabled;
        IsOtpEnabled = isOtpEnabled;
        IsEmailOtpEnabled = isEmailOtpEnabled;
        IsPhoneOtpEnabled = isPhoneOtpEnabled;
        OtpExpirationMinutes = otpExpirationMinutes;
        OtpMaxAttempts = otpMaxAttempts;

        LoginRateLimitPermitLimit = loginRateLimitPermitLimit;
        LoginRateLimitWindowSeconds = loginRateLimitWindowSeconds;
        LoginRateLimitQueueLimit = loginRateLimitQueueLimit;
        LoginRateLimitCooldownSeconds = loginRateLimitCooldownSeconds;
        LoginLockoutFailureThreshold = loginLockoutFailureThreshold;
        LoginLockoutDurationSeconds = loginLockoutDurationSeconds;

        RefreshTokenRateLimitPermitLimit = refreshTokenRateLimitPermitLimit;
        RefreshTokenRateLimitWindowSeconds = refreshTokenRateLimitWindowSeconds;
        RefreshTokenRateLimitQueueLimit = refreshTokenRateLimitQueueLimit;
        RefreshTokenRateLimitCooldownSeconds = refreshTokenRateLimitCooldownSeconds;

        SensitiveAdminRateLimitPermitLimit = sensitiveAdminRateLimitPermitLimit;
        SensitiveAdminRateLimitWindowSeconds = sensitiveAdminRateLimitWindowSeconds;
        SensitiveAdminRateLimitQueueLimit = sensitiveAdminRateLimitQueueLimit;
        SensitiveAdminRateLimitCooldownSeconds = sensitiveAdminRateLimitCooldownSeconds;

        AllowedIpRanges = allowed;
        BlockedIpRanges = blocked;
    }

    private static string[] NormalizeRanges(IReadOnlyCollection<string>? ranges)
    {
        if (ranges is null || ranges.Count == 0)
            return Array.Empty<string>();

        return ranges
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(ValidateIpRange)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ValidateIpRange(string range)
    {
        if (IPAddress.TryParse(range, out _))
            return range;

        var slashIndex = range.IndexOf('/');
        if (slashIndex > 0)
        {
            var ipPart = range[..slashIndex];
            var prefixPart = range[(slashIndex + 1)..];

            if (IPAddress.TryParse(ipPart, out var ipAddress) &&
                int.TryParse(prefixPart, out var prefix))
            {
                var maxPrefix = ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
                if (prefix >= 0 && prefix <= maxPrefix)
                    return range;
            }
        }

        throw new DomainException($"IP range '{range}' is invalid.");
    }

    private static string JoinRanges(IEnumerable<string> ranges)
        => string.Join(",", ranges);

    private static string[] SplitRanges(string ranges)
        => string.IsNullOrWhiteSpace(ranges)
            ? Array.Empty<string>()
            : ranges.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
