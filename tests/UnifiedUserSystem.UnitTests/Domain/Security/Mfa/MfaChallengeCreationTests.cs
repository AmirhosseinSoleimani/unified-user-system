
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Domain.Security.Mfa;

[Trait("Category", "Domain")]
[Trait("Entity", "MfaChallenge")]
public sealed class MfaChallengeCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateChallenge()
    {
        var challenge = MfaChallengeTestFactory.Create();

        challenge.Id.Should().NotBeEmpty();
        challenge.UserId.Should().Be(MfaChallengeTestFactory.UserId);
        challenge.Channel.Should().Be(MfaChannel.Email);
        challenge.OtpHash.Should().Be(
            MfaChallengeTestFactory.ValidOtpHash);

        challenge.ExpiresAt.Should().Be(
            MfaChallengeTestFactory.Now.AddMinutes(5));

        challenge.MaxAttempts.Should().Be(3);
        challenge.AttemptCount.Should().Be(0);
        challenge.IsUsed.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();

        challenge.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldInitializeAuditFields()
    {
        var challenge = MfaChallengeTestFactory.Create(
            actorUserId: MfaChallengeTestFactory.ActorUserId);

        challenge.CreatedAt.Should().Be(MfaChallengeTestFactory.Now);
        challenge.UpdatedAt.Should().Be(MfaChallengeTestFactory.Now);

        challenge.CreatedByUserId.Should()
            .Be(MfaChallengeTestFactory.ActorUserId);

        challenge.UpdatedByUserId.Should()
            .Be(MfaChallengeTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldUseChallengeUserIdForAudit()
    {
        var challenge = MfaChallengeTestFactory.Create(
            actorUserId: null);

        challenge.CreatedByUserId.Should()
            .Be(MfaChallengeTestFactory.UserId);

        challenge.UpdatedByUserId.Should()
            .Be(MfaChallengeTestFactory.UserId);
    }

    [Fact]
    public void Create_ShouldGenerateDifferentIdsForDifferentChallenges()
    {
        var first = MfaChallengeTestFactory.Create();
        var second = MfaChallengeTestFactory.Create();

        first.Id.Should().NotBe(second.Id);
    }

    [Theory]
    [InlineData(MfaChannel.Email)]
    [InlineData(MfaChannel.Phone)]
    public void Create_WithValidChannel_ShouldSetChannel(
        MfaChannel channel)
    {
        var challenge = MfaChallengeTestFactory.Create(
            channel: channel);

        challenge.Channel.Should().Be(channel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Create_WithUndefinedChannel_ShouldThrow(
        int channelValue)
    {
        var channel = (MfaChannel)channelValue;

        var action = () => MfaChallengeTestFactory.Create(
            channel: channel);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*MFA channel is invalid*");
    }

    [Fact]
    public void Create_WhenUserIdEmpty_ShouldThrow()
    {
        var action = () => MfaChallengeTestFactory.Create(
            userId: Guid.Empty);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*UserId is invalid*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenOtpHashMissing_ShouldThrow(
        string? otpHash)
    {
        var action = () => MfaChallengeTestFactory.Create(
            otpHash: otpHash!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenOtpHashExceedsMaximumLength_ShouldThrow()
    {
        var otpHash = MfaChallengeTestFactory.CreateString(
            MfaChallenge.OtpHashMaxLength + 1);

        var action = () => MfaChallengeTestFactory.Create(
            otpHash: otpHash);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenOtpHashHasMaximumLength_ShouldCreateChallenge()
    {
        var otpHash = MfaChallengeTestFactory.CreateString(
            MfaChallenge.OtpHashMaxLength);

        var challenge = MfaChallengeTestFactory.Create(
            otpHash: otpHash);

        challenge.OtpHash.Should().HaveLength(
            MfaChallenge.OtpHashMaxLength);
    }

    [Fact]
    public void Create_ShouldTrimOtpHash()
    {
        var challenge = MfaChallengeTestFactory.Create(
            otpHash: " valid-hash ");

        challenge.OtpHash.Should().Be("valid-hash");
    }

    [Fact]
    public void Create_WhenExpiresAtEqualsNow_ShouldThrow()
    {
        var action = () => MfaChallengeTestFactory.Create(
            expiresAt: MfaChallengeTestFactory.Now);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*ExpiresAt must be in the future*");
    }

    [Fact]
    public void Create_WhenExpiresAtIsBeforeNow_ShouldThrow()
    {
        var action = () => MfaChallengeTestFactory.Create(
            expiresAt: MfaChallengeTestFactory.Now.AddTicks(-1));

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*ExpiresAt must be in the future*");
    }

    [Fact]
    public void Create_WhenExpiresAtIsAfterNow_ShouldCreateChallenge()
    {
        var expiresAt = MfaChallengeTestFactory.Now.AddTicks(1);

        var challenge = MfaChallengeTestFactory.Create(
            expiresAt: expiresAt);

        challenge.ExpiresAt.Should().Be(expiresAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WhenMaxAttemptsIsNotPositive_ShouldThrow(
        int maxAttempts)
    {
        var action = () => MfaChallengeTestFactory.Create(
            maxAttempts: maxAttempts);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*MaxAttempts must be greater than 0*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Create_WhenMaxAttemptsPositive_ShouldCreateChallenge(
        int maxAttempts)
    {
        var challenge = MfaChallengeTestFactory.Create(
            maxAttempts: maxAttempts);

        challenge.MaxAttempts.Should().Be(maxAttempts);
    }
}
