using FluentAssertions;
using SecuritySettingsEntity =
    UnifiedUserSystem.src.Domain.Security.Entities.SecuritySettings;

namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

[Trait("Category", "Domain")]
[Trait("Entity", "SecuritySettings")]
public sealed class SecuritySettingsCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSettings()
    {
        var data = new SecuritySettingsTestData();

        var settings = data.Create();

        settings.Id.Should().Be(SecuritySettingsEntity.SingletonId);

        settings.IsMfaEnabled.Should().BeTrue();
        settings.IsOtpEnabled.Should().BeTrue();
        settings.IsEmailOtpEnabled.Should().BeTrue();
        settings.IsPhoneOtpEnabled.Should().BeTrue();

        settings.OtpExpirationMinutes.Should().Be(5);
        settings.OtpMaxAttempts.Should().Be(5);
    }

    [Fact]
    public void Create_ShouldSetLoginRateLimitSettings()
    {
        var data = new SecuritySettingsTestData
        {
            LoginRateLimitPermitLimit = 20,
            LoginRateLimitWindowSeconds = 120,
            LoginRateLimitQueueLimit = 5,
            LoginRateLimitCooldownSeconds = 10,
            LoginLockoutFailureThreshold = 7,
            LoginLockoutDurationSeconds = 1800
        };

        var settings = data.Create();

        settings.LoginRateLimitPermitLimit.Should().Be(20);
        settings.LoginRateLimitWindowSeconds.Should().Be(120);
        settings.LoginRateLimitQueueLimit.Should().Be(5);
        settings.LoginRateLimitCooldownSeconds.Should().Be(10);
        settings.LoginLockoutFailureThreshold.Should().Be(7);
        settings.LoginLockoutDurationSeconds.Should().Be(1800);
    }

    [Fact]
    public void Create_ShouldSetRefreshTokenRateLimitSettings()
    {
        var data = new SecuritySettingsTestData
        {
            RefreshTokenRateLimitPermitLimit = 15,
            RefreshTokenRateLimitWindowSeconds = 90,
            RefreshTokenRateLimitQueueLimit = 4,
            RefreshTokenRateLimitCooldownSeconds = 8
        };

        var settings = data.Create();

        settings.RefreshTokenRateLimitPermitLimit.Should().Be(15);
        settings.RefreshTokenRateLimitWindowSeconds.Should().Be(90);
        settings.RefreshTokenRateLimitQueueLimit.Should().Be(4);
        settings.RefreshTokenRateLimitCooldownSeconds.Should().Be(8);
    }

    [Fact]
    public void Create_ShouldSetSensitiveAdminRateLimitSettings()
    {
        var data = new SecuritySettingsTestData
        {
            SensitiveAdminRateLimitPermitLimit = 50,
            SensitiveAdminRateLimitWindowSeconds = 300,
            SensitiveAdminRateLimitQueueLimit = 2,
            SensitiveAdminRateLimitCooldownSeconds = 30
        };

        var settings = data.Create();

        settings.SensitiveAdminRateLimitPermitLimit.Should().Be(50);
        settings.SensitiveAdminRateLimitWindowSeconds.Should().Be(300);
        settings.SensitiveAdminRateLimitQueueLimit.Should().Be(2);
        settings.SensitiveAdminRateLimitCooldownSeconds.Should().Be(30);
    }

    [Fact]
    public void Create_ShouldInitializeAuditFields()
    {
        var data = new SecuritySettingsTestData();

        var settings = data.Create();

        settings.CreatedAt.Should().Be(data.NowUtc);
        settings.UpdatedAt.Should().Be(data.NowUtc);
        settings.CreatedByUserId.Should().Be(data.ActorUserId);
        settings.UpdatedByUserId.Should().Be(data.ActorUserId);
    }

    [Fact]
    public void Create_ShouldNotMarkSettingsDeleted()
    {
        var settings = new SecuritySettingsTestData().Create();

        settings.IsDeleted.Should().BeFalse();
        settings.DeletedAt.Should().BeNull();
        settings.DeletedByUserId.Should().BeNull();
    }
}
