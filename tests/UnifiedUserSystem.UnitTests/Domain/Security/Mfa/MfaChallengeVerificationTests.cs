
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Security.Mfa;

[Trait("Category", "Domain")]
[Trait("Entity", "MfaChallenge")]
public sealed class MfaChallengeVerificationTests
{
    [Fact]
    public void IsExpired_WhenCurrentTimeBeforeExpiration_ShouldReturnFalse()
    {
        var challenge = MfaChallengeTestFactory.Create();

        var result = challenge.IsExpired(
            MfaChallengeTestFactory.Now.AddMinutes(4));

        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenCurrentTimeEqualsExpiration_ShouldReturnTrue()
    {
        var expiresAt = MfaChallengeTestFactory.Now.AddMinutes(5);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        var result = challenge.IsExpired(expiresAt);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenCurrentTimeAfterExpiration_ShouldReturnTrue()
    {
        var challenge = MfaChallengeTestFactory.Create();

        var result = challenge.IsExpired(
            MfaChallengeTestFactory.Now.AddMinutes(6));

        result.Should().BeTrue();
    }

    [Fact]
    public void CanVerify_WhenChallengeIsValid_ShouldReturnTrue()
    {
        var challenge = MfaChallengeTestFactory.Create();

        var result = challenge.CanVerify(
            MfaChallengeTestFactory.Now.AddMinutes(1));

        result.Should().BeTrue();
    }

    [Fact]
    public void CanVerify_WhenChallengeExpired_ShouldReturnFalse()
    {
        var expiresAt = MfaChallengeTestFactory.Now.AddMinutes(5);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        var result = challenge.CanVerify(expiresAt);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanVerify_WhenChallengeAlreadyUsed_ShouldReturnFalse()
    {
        var challenge = MfaChallengeTestFactory.Create();

        challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        challenge.CanVerify(
                MfaChallengeTestFactory.Now.AddMinutes(2))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void CanVerify_WhenAttemptLimitReached_ShouldReturnFalse()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 2);

        challenge.Verify(
            otpMatches: false,
            nowUtc: MfaChallengeTestFactory.Now.AddSeconds(1),
            actorUserId: null);

        challenge.Verify(
            otpMatches: false,
            nowUtc: MfaChallengeTestFactory.Now.AddSeconds(2),
            actorUserId: null);

        challenge.CanVerify(
                MfaChallengeTestFactory.Now.AddSeconds(3))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Verify_WhenOtpMatches_ShouldMarkChallengeAsUsed()
    {
        var challenge = MfaChallengeTestFactory.Create();
        var verifiedAt = MfaChallengeTestFactory.Now.AddMinutes(1);
        var actorId = Guid.NewGuid();

        challenge.Verify(
            otpMatches: true,
            nowUtc: verifiedAt,
            actorUserId: actorId);

        challenge.IsUsed.Should().BeTrue();
        challenge.VerifiedAt.Should().Be(verifiedAt);
        challenge.AttemptCount.Should().Be(1);

        challenge.UpdatedAt.Should().Be(verifiedAt);
        challenge.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Verify_WhenOtpMatchesWithoutActor_ShouldUseUserIdForAudit()
    {
        var challenge = MfaChallengeTestFactory.Create();

        challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        challenge.UpdatedByUserId.Should()
            .Be(MfaChallengeTestFactory.UserId);
    }

    [Fact]
    public void Verify_WhenOtpDoesNotMatch_ShouldIncrementAttemptCount()
    {
        var challenge = MfaChallengeTestFactory.Create();
        var attemptedAt = MfaChallengeTestFactory.Now.AddMinutes(1);
        var actorId = Guid.NewGuid();

        challenge.Verify(
            otpMatches: false,
            nowUtc: attemptedAt,
            actorUserId: actorId);

        challenge.AttemptCount.Should().Be(1);
        challenge.IsUsed.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();

        challenge.UpdatedAt.Should().Be(attemptedAt);
        challenge.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Verify_WhenOtpDoesNotMatchWithoutActor_ShouldUseUserIdForAudit()
    {
        var challenge = MfaChallengeTestFactory.Create();

        challenge.Verify(
            otpMatches: false,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        challenge.UpdatedByUserId.Should()
            .Be(MfaChallengeTestFactory.UserId);
    }

    [Fact]
    public void Verify_WithMultipleWrongAttempts_ShouldIncrementEachTime()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 3);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(1),
            null);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(2),
            null);

        challenge.AttemptCount.Should().Be(2);
        challenge.IsUsed.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void Verify_AfterWrongAttemptWithCorrectOtp_ShouldSucceed()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 3);

        challenge.Verify(
            otpMatches: false,
            nowUtc: MfaChallengeTestFactory.Now.AddSeconds(1),
            actorUserId: null);

        var verifiedAt = MfaChallengeTestFactory.Now.AddSeconds(2);

        challenge.Verify(
            otpMatches: true,
            nowUtc: verifiedAt,
            actorUserId: null);

        challenge.AttemptCount.Should().Be(2);
        challenge.IsUsed.Should().BeTrue();
        challenge.VerifiedAt.Should().Be(verifiedAt);
    }

    [Fact]
    public void Verify_WhenAttemptLimitReached_ShouldThrow()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 2);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(1),
            null);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(2),
            null);

        var action = () => challenge.Verify(
            true,
            MfaChallengeTestFactory.Now.AddSeconds(3),
            null);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*retry limit has been reached*");
    }

    [Fact]
    public void Verify_WhenAttemptLimitReached_ShouldNotChangeState()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 1);

        var firstAttemptAt =
            MfaChallengeTestFactory.Now.AddSeconds(1);

        challenge.Verify(
            false,
            firstAttemptAt,
            MfaChallengeTestFactory.ActorUserId);

        var previousAttemptCount = challenge.AttemptCount;
        var previousUpdatedAt = challenge.UpdatedAt;
        var previousUpdatedBy = challenge.UpdatedByUserId;

        var action = () => challenge.Verify(
            true,
            MfaChallengeTestFactory.Now.AddSeconds(2),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        challenge.AttemptCount.Should().Be(previousAttemptCount);
        challenge.IsUsed.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();
        challenge.UpdatedAt.Should().Be(previousUpdatedAt);
        challenge.UpdatedByUserId.Should().Be(previousUpdatedBy);
    }

    [Fact]
    public void Verify_WhenChallengeExpired_ShouldThrow()
    {
        var expiresAt =
            MfaChallengeTestFactory.Now.AddMinutes(5);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        var action = () => challenge.Verify(
            otpMatches: true,
            nowUtc: expiresAt,
            actorUserId: null);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*MFA challenge has expired*");
    }

    [Fact]
    public void Verify_WhenChallengeExpired_ShouldNotChangeState()
    {
        var expiresAt =
            MfaChallengeTestFactory.Now.AddMinutes(5);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        var previousUpdatedAt = challenge.UpdatedAt;
        var previousUpdatedBy = challenge.UpdatedByUserId;

        var action = () => challenge.Verify(
            otpMatches: false,
            nowUtc: expiresAt,
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();

        challenge.AttemptCount.Should().Be(0);
        challenge.IsUsed.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();
        challenge.UpdatedAt.Should().Be(previousUpdatedAt);
        challenge.UpdatedByUserId.Should().Be(previousUpdatedBy);
    }

    [Fact]
    public void Verify_WhenChallengeAlreadyUsed_ShouldThrow()
    {
        var challenge = MfaChallengeTestFactory.Create();

        challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        var action = () => challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(2),
            actorUserId: null);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*already been used*");
    }

    [Fact]
    public void Verify_WhenChallengeAlreadyUsed_ShouldNotChangeState()
    {
        var challenge = MfaChallengeTestFactory.Create();

        var firstVerifiedAt =
            MfaChallengeTestFactory.Now.AddMinutes(1);

        challenge.Verify(
            otpMatches: true,
            nowUtc: firstVerifiedAt,
            actorUserId: MfaChallengeTestFactory.ActorUserId);

        var previousAttemptCount = challenge.AttemptCount;
        var previousUpdatedAt = challenge.UpdatedAt;
        var previousUpdatedBy = challenge.UpdatedByUserId;
        var previousVerifiedAt = challenge.VerifiedAt;

        var action = () => challenge.Verify(
            otpMatches: false,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(2),
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();

        challenge.AttemptCount.Should().Be(previousAttemptCount);
        challenge.IsUsed.Should().BeTrue();
        challenge.VerifiedAt.Should().Be(previousVerifiedAt);
        challenge.UpdatedAt.Should().Be(previousUpdatedAt);
        challenge.UpdatedByUserId.Should().Be(previousUpdatedBy);
    }

    [Fact]
    public void SuccessfulVerification_ShouldConsumeOneAttempt()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 3);

        challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        challenge.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void LastAvailableAttempt_WithCorrectOtp_ShouldSucceed()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 3);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(1),
            null);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(2),
            null);

        challenge.Verify(
            true,
            MfaChallengeTestFactory.Now.AddSeconds(3),
            null);

        challenge.AttemptCount.Should().Be(3);
        challenge.IsUsed.Should().BeTrue();
        challenge.VerifiedAt.Should()
            .Be(MfaChallengeTestFactory.Now.AddSeconds(3));
    }

    [Fact]
    public void LastAvailableAttempt_WithWrongOtp_ShouldReachRetryLimit()
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: 3);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(1),
            null);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(2),
            null);

        challenge.Verify(
            false,
            MfaChallengeTestFactory.Now.AddSeconds(3),
            null);

        challenge.AttemptCount.Should().Be(3);
        challenge.IsUsed.Should().BeFalse();

        challenge.CanVerify(
                MfaChallengeTestFactory.Now.AddSeconds(4))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Verify_WhenChallengeUsedAndExpired_ShouldReportUsedFirst()
    {
        var expiresAt =
            MfaChallengeTestFactory.Now.AddMinutes(5);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        challenge.Verify(
            otpMatches: true,
            nowUtc: MfaChallengeTestFactory.Now.AddMinutes(1),
            actorUserId: null);

        var action = () => challenge.Verify(
            otpMatches: true,
            nowUtc: expiresAt.AddMinutes(1),
            actorUserId: null);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*already been used*");
    }
}
