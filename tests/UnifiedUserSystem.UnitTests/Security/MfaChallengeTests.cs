using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Security;

public sealed class MfaChallengeTests
{
    [Fact]
    public void Create_Should_Reject_Expired_Challenge()
    {
        var now = DateTimeOffset.UtcNow;

        Action act = () => MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Email,
            "hash",
            now,
            now,
            3,
            null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Verify_Should_Increment_Attempt_For_Wrong_Otp()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Email,
            "hash",
            now,
            now.AddMinutes(5),
            3,
            null);

        challenge.Verify(false, now.AddSeconds(10), null);

        challenge.AttemptCount.Should().Be(1);
        challenge.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void Verify_Should_Block_After_Max_Attempts()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Email,
            "hash",
            now,
            now.AddMinutes(5),
            1,
            null);

        challenge.Verify(false, now.AddSeconds(10), null);

        Action act = () => challenge.Verify(true, now.AddSeconds(20), null);

        act.Should().Throw<DomainException>()
            .WithMessage("*retry limit*");
    }

    [Fact]
    public void Verify_Should_Mark_Challenge_Used_For_Correct_Otp()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Phone,
            "hash",
            now,
            now.AddMinutes(5),
            3,
            null);

        challenge.Verify(true, now.AddSeconds(10), null);

        challenge.IsUsed.Should().BeTrue();
        challenge.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Verify_Should_Reject_Used_Challenge()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Phone,
            "hash",
            now,
            now.AddMinutes(5),
            3,
            null);

        challenge.Verify(true, now.AddSeconds(10), null);

        Action act = () => challenge.Verify(true, now.AddSeconds(20), null);

        act.Should().Throw<DomainException>()
            .WithMessage("*already been used*");
    }

    [Fact]
    public void Verify_Should_Reject_Expired_Challenge()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = MfaChallenge.Create(
            Guid.NewGuid(),
            MfaChannel.Email,
            "hash",
            now,
            now.AddMinutes(5),
            3,
            null);

        Action act = () => challenge.Verify(true, now.AddMinutes(6), null);

        act.Should().Throw<DomainException>()
            .WithMessage("*expired*");
    }
}