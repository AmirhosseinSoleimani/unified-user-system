using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.UnitTests.TestHelpers;

internal static class TestSecuritySettingsFactory
{
    internal static readonly DateTimeOffset Now =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static SecuritySettings Create(
        int otpExpirationMinutes = 5,
        int otpMaxAttempts = 5,
        int loginPermitLimit = 10,
        int loginWindowSeconds = 60,
        int loginFailureThreshold = 5,
        int loginLockoutSeconds = 900,
        bool isMfaEnabled = false,
        bool isOtpEnabled = true,
        bool isEmailOtpEnabled = true,
        bool isPhoneOtpEnabled = true,
        IReadOnlyCollection<string>? allowedIpRanges = null,
        IReadOnlyCollection<string>? blockedIpRanges = null)
    {
        return SecuritySettings.Create(
            isMfaEnabled: isMfaEnabled,
            isOtpEnabled: isOtpEnabled,
            isEmailOtpEnabled: isEmailOtpEnabled,
            isPhoneOtpEnabled: isPhoneOtpEnabled,
            otpExpirationMinutes: otpExpirationMinutes,
            otpMaxAttempts: otpMaxAttempts,
            loginRateLimitPermitLimit: loginPermitLimit,
            loginRateLimitWindowSeconds: loginWindowSeconds,
            loginRateLimitQueueLimit: 0,
            loginRateLimitCooldownSeconds: 2,
            loginLockoutFailureThreshold: loginFailureThreshold,
            loginLockoutDurationSeconds: loginLockoutSeconds,
            refreshTokenRateLimitPermitLimit: 10,
            refreshTokenRateLimitWindowSeconds: 60,
            refreshTokenRateLimitQueueLimit: 0,
            refreshTokenRateLimitCooldownSeconds: 2,
            sensitiveAdminRateLimitPermitLimit: 30,
            sensitiveAdminRateLimitWindowSeconds: 60,
            sensitiveAdminRateLimitQueueLimit: 0,
            sensitiveAdminRateLimitCooldownSeconds: 2,
            allowedIpRanges: allowedIpRanges ?? Array.Empty<string>(),
            blockedIpRanges: blockedIpRanges ?? Array.Empty<string>(),
            nowUtc: Now,
            actorUserId: Guid.NewGuid());
    }
}
