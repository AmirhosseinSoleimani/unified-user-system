
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

[Trait("Category", "Domain")]
[Trait("Entity", "SecuritySettings")]
public sealed class SecuritySettingsUpdateTests
{
    [Fact]
    public void Update_WithValidData_ShouldUpdateAllSettings()
    {
        var settings = new SecuritySettingsTestData().Create();

        var updateData = new SecuritySettingsTestData
        {
            IsMfaEnabled = false,
            IsOtpEnabled = false,
            IsEmailOtpEnabled = false,
            IsPhoneOtpEnabled = false,

            OtpExpirationMinutes = 15,
            OtpMaxAttempts = 8,

            LoginRateLimitPermitLimit = 20,
            LoginRateLimitWindowSeconds = 120,
            LoginRateLimitQueueLimit = 5,
            LoginRateLimitCooldownSeconds = 10,
            LoginLockoutFailureThreshold = 10,
            LoginLockoutDurationSeconds = 1800,

            RefreshTokenRateLimitPermitLimit = 15,
            RefreshTokenRateLimitWindowSeconds = 90,
            RefreshTokenRateLimitQueueLimit = 3,
            RefreshTokenRateLimitCooldownSeconds = 4,

            SensitiveAdminRateLimitPermitLimit = 50,
            SensitiveAdminRateLimitWindowSeconds = 300,
            SensitiveAdminRateLimitQueueLimit = 2,
            SensitiveAdminRateLimitCooldownSeconds = 20,

            AllowedIpRanges = ["172.16.0.0/12"],
            BlockedIpRanges = ["198.51.100.0/24"],

            NowUtc = SecuritySettingsTestData.DefaultNow.AddHours(1),
            ActorUserId = Guid.NewGuid()
        };

        updateData.Update(settings);

        settings.IsMfaEnabled.Should().BeFalse();
        settings.IsOtpEnabled.Should().BeFalse();
        settings.IsEmailOtpEnabled.Should().BeFalse();
        settings.IsPhoneOtpEnabled.Should().BeFalse();

        settings.OtpExpirationMinutes.Should().Be(15);
        settings.OtpMaxAttempts.Should().Be(8);

        settings.LoginRateLimitPermitLimit.Should().Be(20);
        settings.LoginRateLimitWindowSeconds.Should().Be(120);
        settings.LoginRateLimitQueueLimit.Should().Be(5);
        settings.LoginRateLimitCooldownSeconds.Should().Be(10);
        settings.LoginLockoutFailureThreshold.Should().Be(10);
        settings.LoginLockoutDurationSeconds.Should().Be(1800);

        settings.RefreshTokenRateLimitPermitLimit.Should().Be(15);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(90);
        settings.RefreshTokenRateLimitQueueLimit.Should().Be(3);
        settings.RefreshTokenRateLimitCooldownSeconds.Should().Be(4);

        settings.SensitiveAdminRateLimitPermitLimit.Should().Be(50);
        settings.SensitiveAdminRateLimitWindowSeconds.Should().Be(300);
        settings.SensitiveAdminRateLimitQueueLimit.Should().Be(2);
        settings.SensitiveAdminRateLimitCooldownSeconds.Should().Be(20);

        settings.GetAllowedIpRanges()
            .Should()
            .Equal("172.16.0.0/12");

        settings.GetBlockedIpRanges()
            .Should()
            .Equal("198.51.100.0/24");

        settings.UpdatedAt.Should().Be(updateData.NowUtc);
        settings.UpdatedByUserId.Should().Be(updateData.ActorUserId);
    }

    [Fact]
    public void Update_ShouldPreserveCreationAudit()
    {
        var creationData = new SecuritySettingsTestData
        {
            ActorUserId = SecuritySettingsTestData.DefaultActorId
        };

        var settings = creationData.Create();

        var originalCreatedAt = settings.CreatedAt;
        var originalCreatedBy = settings.CreatedByUserId;

        var updateData = new SecuritySettingsTestData
        {
            NowUtc = creationData.NowUtc.AddHours(1),
            ActorUserId = Guid.NewGuid()
        };

        updateData.Update(settings);

        settings.CreatedAt.Should().Be(originalCreatedAt);
        settings.CreatedByUserId.Should().Be(originalCreatedBy);
    }

    [Fact]
    public void Update_WithSameValues_ShouldStillTouchAudit()
    {
        var creationData = new SecuritySettingsTestData();
        var settings = creationData.Create();

        var updatedAt = creationData.NowUtc.AddMinutes(5);
        var actorId = Guid.NewGuid();

        var updateData = new SecuritySettingsTestData
        {
            NowUtc = updatedAt,
            ActorUserId = actorId
        };

        updateData.Update(settings);

        settings.UpdatedAt.Should().Be(updatedAt);
        settings.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Update_WithoutActor_ShouldSetUpdatedActorNull()
    {
        var settings = new SecuritySettingsTestData().Create();

        var updateData = new SecuritySettingsTestData
        {
            NowUtc = SecuritySettingsTestData.DefaultNow.AddMinutes(1),
            ActorUserId = null
        };

        updateData.Update(settings);

        settings.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Update_WhenValidationFails_ShouldNotTouchAudit()
    {
        var settings = new SecuritySettingsTestData().Create();

        var originalUpdatedAt = settings.UpdatedAt;
        var originalUpdatedBy = settings.UpdatedByUserId;

        var updateData = new SecuritySettingsTestData
        {
            OtpExpirationMinutes = 0,
            NowUtc = SecuritySettingsTestData.DefaultNow.AddHours(1),
            ActorUserId = Guid.NewGuid()
        };

        var action = () => updateData.Update(settings);

        action.Should().Throw<DomainException>();

        settings.UpdatedAt.Should().Be(originalUpdatedAt);
        settings.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Update_WhenEarlyValidationFails_ShouldNotChangeProperties()
    {
        var settings = new SecuritySettingsTestData().Create();

        var originalOtpExpiration = settings.OtpExpirationMinutes;
        var originalOtpMaxAttempts = settings.OtpMaxAttempts;
        var originalAllowedRanges = settings.AllowedIpRanges;
        var originalBlockedRanges = settings.BlockedIpRanges;

        var updateData = new SecuritySettingsTestData
        {
            OtpExpirationMinutes = 0,
            OtpMaxAttempts = 99,
            AllowedIpRanges = ["172.16.0.0/12"],
            BlockedIpRanges = ["198.51.100.0/24"]
        };

        var action = () => updateData.Update(settings);

        action.Should().Throw<DomainException>();

        settings.OtpExpirationMinutes.Should()
            .Be(originalOtpExpiration);

        settings.OtpMaxAttempts.Should()
            .Be(originalOtpMaxAttempts);

        settings.AllowedIpRanges.Should()
            .Be(originalAllowedRanges);

        settings.BlockedIpRanges.Should()
            .Be(originalBlockedRanges);
    }

    [Fact]
    public void Update_WhenIpConflictDetected_ShouldNotChangeProperties()
    {
        var settings = new SecuritySettingsTestData().Create();

        var originalMfaEnabled = settings.IsMfaEnabled;
        var originalAllowedRanges = settings.AllowedIpRanges;
        var originalBlockedRanges = settings.BlockedIpRanges;
        var originalUpdatedAt = settings.UpdatedAt;

        var updateData = new SecuritySettingsTestData
        {
            IsMfaEnabled = !originalMfaEnabled,
            AllowedIpRanges = ["10.0.0.0/8"],
            BlockedIpRanges = ["10.0.0.0/8"],
            NowUtc = SecuritySettingsTestData.DefaultNow.AddHours(1)
        };

        var action = () => updateData.Update(settings);

        action.Should().Throw<DomainException>();

        settings.IsMfaEnabled.Should().Be(originalMfaEnabled);
        settings.AllowedIpRanges.Should().Be(originalAllowedRanges);
        settings.BlockedIpRanges.Should().Be(originalBlockedRanges);
        settings.UpdatedAt.Should().Be(originalUpdatedAt);
    }
}
