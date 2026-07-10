using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Security;

public class SecuritySettingsInvariantTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    [Fact]
    public void CreateDefault_Should_CreateSafeDefaults()
    {
        var settings = SecuritySettings.CreateDefault(Now, actorUserId: null);

        settings.Id.Should().Be(SecuritySettings.SingletonId);
        settings.IsMfaEnabled.Should().BeFalse();
        settings.IsOtpEnabled.Should().BeTrue();
        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeTrue();
        settings.OtpExpirationMinutes.Should().Be(5);
        settings.OtpMaxAttempts.Should().Be(5);
        settings.LoginRateLimitPermitLimit.Should().Be(10);
        settings.LoginRateLimitWindowSeconds.Should().Be(60);
        settings.RefreshTokenRateLimitPermitLimit.Should().Be(10);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(60);
        settings.AllowedIpRanges.Should().BeEmpty();
        settings.BlockedIpRanges.Should().BeEmpty();
        settings.GetAllowedIpRanges().Should().BeEmpty();
        settings.GetBlockedIpRanges().Should().BeEmpty();
        settings.CreatedAt.Should().Be(Now);
        settings.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_Should_AcceptValidSettings()
    {
        var settings = CreateValidSettings(
            allowedIpRanges: new[] { "192.168.1.1", "10.0.0.0/24" },
            blockedIpRanges: new[] { "172.16.0.1" });

        settings.IsMfaEnabled.Should().BeTrue();
        settings.IsOtpEnabled.Should().BeTrue();
        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeTrue();
        settings.OtpExpirationMinutes.Should().Be(10);
        settings.OtpMaxAttempts.Should().Be(3);
        settings.LoginRateLimitPermitLimit.Should().Be(5);
        settings.LoginRateLimitWindowSeconds.Should().Be(30);
        settings.RefreshTokenRateLimitPermitLimit.Should().Be(8);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(45);
        settings.GetAllowedIpRanges().Should().Equal("192.168.1.1", "10.0.0.0/24");
        settings.GetBlockedIpRanges().Should().Equal("172.16.0.1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenOtpExpirationMinutesIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(otpExpirationMinutes: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*OtpExpirationMinutes*greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenOtpMaxAttemptsIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(otpMaxAttempts: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*OtpMaxAttempts*greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenLoginRateLimitPermitLimitIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(loginRateLimitPermitLimit: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*LoginRateLimitPermitLimit*greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenLoginRateLimitWindowSecondsIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(loginRateLimitWindowSeconds: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*LoginRateLimitWindowSeconds*greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenRefreshTokenRateLimitPermitLimitIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(refreshTokenRateLimitPermitLimit: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*RefreshTokenRateLimitPermitLimit*greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenRefreshTokenRateLimitWindowSecondsIsInvalid_ShouldThrow(int value)
    {
        var act = () => CreateValidSettings(refreshTokenRateLimitWindowSeconds: value);

        act.Should().Throw<DomainException>()
            .WithMessage("*RefreshTokenRateLimitWindowSeconds*greater than 0*");
    }

    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("192.168.1.1/99")]
    [InlineData("10.0.0.0/not-number")]
    public void Create_WhenAllowedIpRangeIsInvalid_ShouldThrow(string ipRange)
    {
        var act = () => CreateValidSettings(allowedIpRanges: new[] { ipRange });

        act.Should().Throw<DomainException>()
            .WithMessage("*IP range*invalid*");
    }

    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("192.168.1.1/99")]
    [InlineData("10.0.0.0/not-number")]
    public void Create_WhenBlockedIpRangeIsInvalid_ShouldThrow(string ipRange)
    {
        var act = () => CreateValidSettings(blockedIpRanges: new[] { ipRange });

        act.Should().Throw<DomainException>()
            .WithMessage("*IP range*invalid*");
    }

    [Fact]
    public void Create_WhenAllowedAndBlockedIpRangesConflict_ShouldThrow()
    {
        var act = () => CreateValidSettings(
            allowedIpRanges: new[] { "192.168.1.1" },
            blockedIpRanges: new[] { "192.168.1.1" });

        act.Should().Throw<DomainException>()
            .WithMessage("*AllowedIpRanges and BlockedIpRanges must not contain the same value*");
    }

    [Fact]
    public void Create_Should_TrimRemoveEmptyAndDeduplicateIpRanges()
    {
        var settings = CreateValidSettings(
            allowedIpRanges: new[] { " 192.168.1.1 ", "", "192.168.1.1", "10.0.0.0/24" },
            blockedIpRanges: new[] { " 172.16.0.1 ", "", "172.16.0.1" });

        settings.GetAllowedIpRanges().Should().Equal("192.168.1.1", "10.0.0.0/24");
        settings.GetBlockedIpRanges().Should().Equal("172.16.0.1");
    }

    [Fact]
    public void Update_Should_UpdateValues_AndTouchAudit()
    {
        var settings = CreateValidSettings();
        var updatedAt = Now.AddMinutes(10);
        var actorUserId = Guid.NewGuid();

        settings.Update(
            isMfaEnabled: false,
            isOtpEnabled: false,
            isEmailOtpEnabled: true,
            isPhoneOtpEnabled: false,
            otpExpirationMinutes: 15,
            otpMaxAttempts: 4,
            loginRateLimitPermitLimit: 6,
            loginRateLimitWindowSeconds: 90,
            refreshTokenRateLimitPermitLimit: 12,
            refreshTokenRateLimitWindowSeconds: 120,
            allowedIpRanges: new[] { "192.168.10.0/24" },
            blockedIpRanges: new[] { "10.10.10.10" },
            nowUtc: updatedAt,
            actorUserId: actorUserId);

        settings.IsMfaEnabled.Should().BeFalse();
        settings.IsOtpEnabled.Should().BeFalse();
        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeFalse();
        settings.OtpExpirationMinutes.Should().Be(15);
        settings.OtpMaxAttempts.Should().Be(4);
        settings.LoginRateLimitPermitLimit.Should().Be(6);
        settings.LoginRateLimitWindowSeconds.Should().Be(90);
        settings.RefreshTokenRateLimitPermitLimit.Should().Be(12);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(120);
        settings.GetAllowedIpRanges().Should().Equal("192.168.10.0/24");
        settings.GetBlockedIpRanges().Should().Equal("10.10.10.10");
        settings.UpdatedAt.Should().Be(updatedAt);
        settings.UpdatedByUserId.Should().Be(actorUserId);
    }

    [Fact]
    public void Update_WhenInvalidValue_ShouldThrow()
    {
        var settings = CreateValidSettings();

        var act = () => settings.Update(
            isMfaEnabled: true,
            isOtpEnabled: true,
            isEmailOtpEnabled: true,
            isPhoneOtpEnabled: true,
            otpExpirationMinutes: 0,
            otpMaxAttempts: 5,
            loginRateLimitPermitLimit: 10,
            loginRateLimitWindowSeconds: 60,
            refreshTokenRateLimitPermitLimit: 10,
            refreshTokenRateLimitWindowSeconds: 60,
            allowedIpRanges: Array.Empty<string>(),
            blockedIpRanges: Array.Empty<string>(),
            nowUtc: Now.AddMinutes(10),
            actorUserId: null);

        act.Should().Throw<DomainException>()
            .WithMessage("*OtpExpirationMinutes*greater than 0*");
    }

    [Fact]
    public void Create_Should_StoreEmailAndPhoneOtpChannelFlags()
    {
        var settings = CreateValidSettings(
            isEmailOtpEnabled: true,
            isPhoneOtpEnabled: false);

        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeFalse();
    }

    private static SecuritySettings CreateValidSettings(
        int otpExpirationMinutes = 10,
        int otpMaxAttempts = 3,
        int loginRateLimitPermitLimit = 5,
        int loginRateLimitWindowSeconds = 30,
        int refreshTokenRateLimitPermitLimit = 8,
        int refreshTokenRateLimitWindowSeconds = 45,
        string[]? allowedIpRanges = null,
        string[]? blockedIpRanges = null,
        bool isEmailOtpEnabled = true,
        bool isPhoneOtpEnabled = true)
    {
        return SecuritySettings.Create(
            isMfaEnabled: true,
            isOtpEnabled: true,
            isEmailOtpEnabled: isEmailOtpEnabled,
            isPhoneOtpEnabled: isPhoneOtpEnabled,
            otpExpirationMinutes: otpExpirationMinutes,
            otpMaxAttempts: otpMaxAttempts,
            loginRateLimitPermitLimit: loginRateLimitPermitLimit,
            loginRateLimitWindowSeconds: loginRateLimitWindowSeconds,
            refreshTokenRateLimitPermitLimit: refreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds: refreshTokenRateLimitWindowSeconds,
            allowedIpRanges: allowedIpRanges ?? Array.Empty<string>(),
            blockedIpRanges: blockedIpRanges ?? Array.Empty<string>(),
            nowUtc: Now,
            actorUserId: null);
    }
}