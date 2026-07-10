
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Domain.Security.Entities;

public sealed class MfaChallenge : AuditableEntity<Guid>
{
    public const int OtpHashMaxLength = 128;

    public Guid UserId { get; private set; }
    public MfaChannel Channel { get; private set; }
    public string OtpHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public int MaxAttempts { get; private set; }
    public int AttemptCount { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }

    public User User { get; private set; } = default!;

    private MfaChallenge()
    {
    }

    public static MfaChallenge Create(
        Guid userId,
        MfaChannel channel,
        string otpHash,
        DateTimeOffset nowUtc,
        DateTimeOffset expiresAt,
        int maxAttempts,
        Guid? actorUserId)
    {
        Guard.True(userId != Guid.Empty, "UserId is invalid.");
        Guard.True(Enum.IsDefined(typeof(MfaChannel), channel), "MFA channel is invalid.");
        Guard.NotEmpty(otpHash, nameof(otpHash));
        Guard.MaxLen(otpHash, OtpHashMaxLength, nameof(otpHash));
        Guard.True(expiresAt > nowUtc, "ExpiresAt must be in the future.");
        Guard.True(maxAttempts > 0, "MaxAttempts must be greater than 0.");

        var challenge = new MfaChallenge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = channel,
            OtpHash = otpHash.Trim(),
            ExpiresAt = expiresAt,
            MaxAttempts = maxAttempts,
            AttemptCount = 0,
            IsUsed = false
        };

        challenge.SetCreated(nowUtc, actorUserId ?? userId);
        return challenge;
    }

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAt;

    public bool CanVerify(DateTimeOffset nowUtc)
        => !IsUsed && !IsExpired(nowUtc) && AttemptCount < MaxAttempts;

    public void Verify(bool otpMatches, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        if (IsUsed)
            throw new DomainException("MFA challenge has already been used.");

        if (IsExpired(nowUtc))
            throw new DomainException("MFA challenge has expired.");

        if (AttemptCount >= MaxAttempts)
            throw new DomainException("MFA challenge retry limit has been reached.");

        if (!otpMatches)
        {
            AttemptCount++;
            Touch(nowUtc, actorUserId ?? UserId);
            return;
        }

        AttemptCount++;
        IsUsed = true;
        VerifiedAt = nowUtc;
        Touch(nowUtc, actorUserId ?? UserId);
    }
}
