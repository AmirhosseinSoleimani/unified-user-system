using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;



namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

[Trait("Category", "Domain")]
[Trait("Feature", "SecurityConfiguration")]
[Trait("Entity", "SecuritySettings")]
public sealed class SecuritySettingsValidationTests
{
    public static TheoryData<
        string,
        Action<SecuritySettingsTestData>,
        string> PositiveValueCases =>
        new()
        {
            {
                nameof(SecuritySettingsTestData.OtpExpirationMinutes),
                data => data.OtpExpirationMinutes = 0,
                "OtpExpirationMinutes must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.OtpMaxAttempts),
                data => data.OtpMaxAttempts = 0,
                "OtpMaxAttempts must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.LoginRateLimitPermitLimit),
                data => data.LoginRateLimitPermitLimit = 0,
                "LoginRateLimitPermitLimit must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.LoginRateLimitWindowSeconds),
                data => data.LoginRateLimitWindowSeconds = 0,
                "LoginRateLimitWindowSeconds must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.LoginLockoutFailureThreshold),
                data => data.LoginLockoutFailureThreshold = 0,
                "LoginLockoutFailureThreshold must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.LoginLockoutDurationSeconds),
                data => data.LoginLockoutDurationSeconds = 0,
                "LoginLockoutDurationSeconds must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.RefreshTokenRateLimitPermitLimit),
                data => data.RefreshTokenRateLimitPermitLimit = 0,
                "RefreshTokenRateLimitPermitLimit must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.RefreshTokenRateLimitWindowSeconds),
                data => data.RefreshTokenRateLimitWindowSeconds = 0,
                "RefreshTokenRateLimitWindowSeconds must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.SensitiveAdminRateLimitPermitLimit),
                data => data.SensitiveAdminRateLimitPermitLimit = 0,
                "SensitiveAdminRateLimitPermitLimit must be greater than 0."
            },
            {
                nameof(SecuritySettingsTestData.SensitiveAdminRateLimitWindowSeconds),
                data => data.SensitiveAdminRateLimitWindowSeconds = 0,
                "SensitiveAdminRateLimitWindowSeconds must be greater than 0."
            }
        };

    public static TheoryData<
        string,
        Action<SecuritySettingsTestData>,
        string> NonNegativeValueCases =>
        new()
        {
            {
                nameof(SecuritySettingsTestData.LoginRateLimitQueueLimit),
                data => data.LoginRateLimitQueueLimit = -1,
                "LoginRateLimitQueueLimit must be greater than or equal to 0."
            },
            {
                nameof(SecuritySettingsTestData.LoginRateLimitCooldownSeconds),
                data => data.LoginRateLimitCooldownSeconds = -1,
                "LoginRateLimitCooldownSeconds must be greater than or equal to 0."
            },
            {
                nameof(SecuritySettingsTestData.RefreshTokenRateLimitQueueLimit),
                data => data.RefreshTokenRateLimitQueueLimit = -1,
                "RefreshTokenRateLimitQueueLimit must be greater than or equal to 0."
            },
            {
                nameof(SecuritySettingsTestData.RefreshTokenRateLimitCooldownSeconds),
                data => data.RefreshTokenRateLimitCooldownSeconds = -1,
                "RefreshTokenRateLimitCooldownSeconds must be greater than or equal to 0."
            },
            {
                nameof(SecuritySettingsTestData.SensitiveAdminRateLimitQueueLimit),
                data => data.SensitiveAdminRateLimitQueueLimit = -1,
                "SensitiveAdminRateLimitQueueLimit must be greater than or equal to 0."
            },
            {
                nameof(SecuritySettingsTestData.SensitiveAdminRateLimitCooldownSeconds),
                data => data.SensitiveAdminRateLimitCooldownSeconds = -1,
                "SensitiveAdminRateLimitCooldownSeconds must be greater than or equal to 0."
            }
        };

    [Theory]
    [MemberData(nameof(PositiveValueCases))]
    public void Create_WhenPositiveSettingIsZero_ShouldThrow(
        string settingName,
        Action<SecuritySettingsTestData> mutate,
        string expectedMessage)
    {
        var data = new SecuritySettingsTestData();

        mutate(data);

        var action = data.Create;

        action.Should()
            .Throw<DomainException>(
                $"because {settingName} must be positive")
            .WithMessage(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(NonNegativeValueCases))]
    public void Create_WhenNonNegativeSettingIsNegative_ShouldThrow(
        string settingName,
        Action<SecuritySettingsTestData> mutate,
        string expectedMessage)
    {
        var data = new SecuritySettingsTestData();

        mutate(data);

        var action = data.Create;

        action.Should()
            .Throw<DomainException>(
                $"because {settingName} cannot be negative")
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void Create_WhenPositiveSettingsEqualOne_ShouldSucceed()
    {
        var data = new SecuritySettingsTestData
        {
            OtpExpirationMinutes = 1,
            OtpMaxAttempts = 1,

            LoginRateLimitPermitLimit = 1,
            LoginRateLimitWindowSeconds = 1,
            LoginLockoutFailureThreshold = 1,
            LoginLockoutDurationSeconds = 1,

            RefreshTokenRateLimitPermitLimit = 1,
            RefreshTokenRateLimitWindowSeconds = 1,

            SensitiveAdminRateLimitPermitLimit = 1,
            SensitiveAdminRateLimitWindowSeconds = 1
        };

        var action = data.Create;

        action.Should().NotThrow();
    }

    [Fact]
    public void Create_WhenQueueAndCooldownSettingsEqualZero_ShouldSucceed()
    {
        var data = new SecuritySettingsTestData
        {
            LoginRateLimitQueueLimit = 0,
            LoginRateLimitCooldownSeconds = 0,

            RefreshTokenRateLimitQueueLimit = 0,
            RefreshTokenRateLimitCooldownSeconds = 0,

            SensitiveAdminRateLimitQueueLimit = 0,
            SensitiveAdminRateLimitCooldownSeconds = 0
        };

        var action = data.Create;

        action.Should().NotThrow();
    }
}
