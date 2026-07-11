using FluentAssertions;
using SecuritySettingsEntity =
    UnifiedUserSystem.src.Domain.Security.Entities.SecuritySettings;

namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

[Trait("Category", "Domain")]
[Trait("Entity", "SecuritySettings")]
public sealed class SecuritySettingsDefaultTests
{
    [Fact]
    public void CreateDefault_ShouldUseSingletonId()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.Id.Should().Be(SecuritySettingsEntity.SingletonId);
    }

    [Fact]
    public void CreateDefault_ShouldApplyExpectedMfaAndOtpSettings()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.IsMfaEnabled.Should().BeFalse();
        settings.IsOtpEnabled.Should().BeTrue();
        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeTrue();

        settings.OtpExpirationMinutes.Should().Be(5);
        settings.OtpMaxAttempts.Should().Be(5);
    }

    [Fact]
    public void CreateDefault_ShouldApplyExpectedLoginSettings()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.LoginRateLimitPermitLimit.Should().Be(10);
        settings.LoginRateLimitWindowSeconds.Should().Be(60);
        settings.LoginRateLimitQueueLimit.Should().Be(0);
        settings.LoginRateLimitCooldownSeconds.Should().Be(2);

        settings.LoginLockoutFailureThreshold.Should().Be(5);
        settings.LoginLockoutDurationSeconds.Should().Be(900);
    }

    [Fact]
    public void CreateDefault_ShouldApplyExpectedRefreshTokenSettings()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.RefreshTokenRateLimitPermitLimit.Should().Be(10);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(60);
        settings.RefreshTokenRateLimitQueueLimit.Should().Be(0);
        settings.RefreshTokenRateLimitCooldownSeconds.Should().Be(2);
    }

    [Fact]
    public void CreateDefault_ShouldApplyExpectedSensitiveAdminSettings()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.SensitiveAdminRateLimitPermitLimit.Should().Be(30);
        settings.SensitiveAdminRateLimitWindowSeconds.Should().Be(60);
        settings.SensitiveAdminRateLimitQueueLimit.Should().Be(0);
        settings.SensitiveAdminRateLimitCooldownSeconds.Should().Be(2);
    }

    [Fact]
    public void CreateDefault_ShouldInitializeEmptyIpRangeCollections()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.AllowedIpRanges.Should().BeEmpty();
        settings.BlockedIpRanges.Should().BeEmpty();

        settings.GetAllowedIpRanges().Should().BeEmpty();
        settings.GetBlockedIpRanges().Should().BeEmpty();
    }

    [Fact]
    public void CreateDefault_ShouldInitializeAuditFields()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            SecuritySettingsTestData.DefaultActorId);

        settings.CreatedAt.Should().Be(SecuritySettingsTestData.DefaultNow);
        settings.UpdatedAt.Should().Be(SecuritySettingsTestData.DefaultNow);

        settings.CreatedByUserId.Should()
            .Be(SecuritySettingsTestData.DefaultActorId);

        settings.UpdatedByUserId.Should()
            .Be(SecuritySettingsTestData.DefaultActorId);
    }

    [Fact]
    public void CreateDefault_WithoutActor_ShouldKeepAuditActorNull()
    {
        var settings = SecuritySettingsEntity.CreateDefault(
            SecuritySettingsTestData.DefaultNow,
            actorUserId: null);

        settings.CreatedByUserId.Should().BeNull();
        settings.UpdatedByUserId.Should().BeNull();
    }
}

