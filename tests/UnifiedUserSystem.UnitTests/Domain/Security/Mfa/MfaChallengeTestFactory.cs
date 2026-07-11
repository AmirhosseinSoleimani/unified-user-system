using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Domain.Security.Mfa;

internal static class MfaChallengeTestFactory
{
    internal static readonly DateTimeOffset Now =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static readonly Guid ActorUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal const string ValidOtpHash = "valid-otp-hash";

    internal static MfaChallenge Create(
        Guid? userId = null,
        MfaChannel channel = MfaChannel.Email,
        string otpHash = ValidOtpHash,
        DateTimeOffset? nowUtc = null,
        DateTimeOffset? expiresAt = null,
        int maxAttempts = 3,
        Guid? actorUserId = null)
    {
        var creationTime = nowUtc ?? Now;

        return MfaChallenge.Create(
            userId: userId ?? UserId,
            channel: channel,
            otpHash: otpHash,
            nowUtc: creationTime,
            expiresAt: expiresAt ?? creationTime.AddMinutes(5),
            maxAttempts: maxAttempts,
            actorUserId: actorUserId);
    }

    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}
