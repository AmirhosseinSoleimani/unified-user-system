using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Security;

public class SecuritySettingsServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    private static readonly Guid ActorUserId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task GetEffectiveAsync_returns_safe_defaults_when_database_settings_do_not_exist()
    {
        var fixture = Fixture(databaseSettings: null);

        var response = await fixture.Service.GetEffectiveAsync();

        response.Id.Should().Be(SecuritySettings.SingletonId);
        response.IsMfaEnabled.Should().BeFalse();
        response.IsOtpEnabled.Should().BeTrue();
        response.IsEmailOtpEnabled.Should().BeTrue();
        response.IsPhoneOtpEnabled.Should().BeTrue();
        response.OtpExpirationMinutes.Should().Be(5);
        response.OtpMaxAttempts.Should().Be(5);

        response.LoginRateLimitPermitLimit.Should().Be(10);
        response.LoginRateLimitWindowSeconds.Should().Be(60);
        response.LoginRateLimitQueueLimit.Should().Be(0);
        response.LoginRateLimitCooldownSeconds.Should().Be(2);
        response.LoginLockoutFailureThreshold.Should().Be(5);
        response.LoginLockoutDurationSeconds.Should().Be(900);

        response.RefreshTokenRateLimitPermitLimit.Should().Be(10);
        response.RefreshTokenRateLimitWindowSeconds.Should().Be(60);
        response.RefreshTokenRateLimitQueueLimit.Should().Be(0);
        response.RefreshTokenRateLimitCooldownSeconds.Should().Be(2);

        response.SensitiveAdminRateLimitPermitLimit.Should().Be(30);
        response.SensitiveAdminRateLimitWindowSeconds.Should().Be(60);
        response.SensitiveAdminRateLimitQueueLimit.Should().Be(0);
        response.SensitiveAdminRateLimitCooldownSeconds.Should().Be(2);

        response.AllowedIpRanges.Should().BeEmpty();
        response.BlockedIpRanges.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAsync_returns_database_settings_when_they_exist()
    {
        var settings = CreateSettings(
            isMfaEnabled: true,
            isOtpEnabled: false,
            otpExpirationMinutes: 10,
            otpMaxAttempts: 3,
            loginRateLimitPermitLimit: 7,
            loginRateLimitWindowSeconds: 30,
            refreshTokenRateLimitPermitLimit: 12,
            refreshTokenRateLimitWindowSeconds: 45,
            allowedIpRanges: new[] { "192.168.1.0/24" },
            blockedIpRanges: new[] { "10.0.0.1" });

        var fixture = Fixture(settings);

        var response = await fixture.Service.GetEffectiveAsync();

        response.Id.Should().Be(SecuritySettings.SingletonId);
        response.IsMfaEnabled.Should().BeTrue();
        response.IsOtpEnabled.Should().BeFalse();
        response.IsEmailOtpEnabled.Should().BeTrue();
        response.IsPhoneOtpEnabled.Should().BeTrue();
        response.OtpExpirationMinutes.Should().Be(10);
        response.OtpMaxAttempts.Should().Be(3);

        response.LoginRateLimitPermitLimit.Should().Be(7);
        response.LoginRateLimitWindowSeconds.Should().Be(30);
        response.LoginRateLimitQueueLimit.Should().Be(0);
        response.LoginRateLimitCooldownSeconds.Should().Be(2);
        response.LoginLockoutFailureThreshold.Should().Be(5);
        response.LoginLockoutDurationSeconds.Should().Be(900);

        response.RefreshTokenRateLimitPermitLimit.Should().Be(12);
        response.RefreshTokenRateLimitWindowSeconds.Should().Be(45);
        response.RefreshTokenRateLimitQueueLimit.Should().Be(0);
        response.RefreshTokenRateLimitCooldownSeconds.Should().Be(2);

        response.SensitiveAdminRateLimitPermitLimit.Should().Be(30);
        response.SensitiveAdminRateLimitWindowSeconds.Should().Be(60);
        response.SensitiveAdminRateLimitQueueLimit.Should().Be(0);
        response.SensitiveAdminRateLimitCooldownSeconds.Should().Be(2);

        response.AllowedIpRanges.Should().Equal("192.168.1.0/24");
        response.BlockedIpRanges.Should().Equal("10.0.0.1");
    }

    [Fact]
    public async Task UpdateAsync_rejects_null_request()
    {
        var fixture = Fixture(databaseSettings: null);

        var act = () => fixture.Service.UpdateAsync(null!);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Request is null*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_otp_expiration()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.OtpExpirationMinutes = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*OtpExpirationMinutes*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_otp_max_attempts()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.OtpMaxAttempts = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*OtpMaxAttempts*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_rate_limit_permit_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginRateLimitPermitLimit = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginRateLimitPermitLimit*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_rate_limit_window_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginRateLimitWindowSeconds = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginRateLimitWindowSeconds*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_rate_limit_queue_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginRateLimitQueueLimit = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginRateLimitQueueLimit*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_rate_limit_cooldown_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginRateLimitCooldownSeconds = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginRateLimitCooldownSeconds*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_lockout_failure_threshold()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginLockoutFailureThreshold = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginLockoutFailureThreshold*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_login_lockout_duration_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.LoginLockoutDurationSeconds = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*LoginLockoutDurationSeconds*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_refresh_token_rate_limit_permit_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.RefreshTokenRateLimitPermitLimit = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*RefreshTokenRateLimitPermitLimit*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_refresh_token_rate_limit_window_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.RefreshTokenRateLimitWindowSeconds = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*RefreshTokenRateLimitWindowSeconds*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_refresh_token_rate_limit_queue_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.RefreshTokenRateLimitQueueLimit = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*RefreshTokenRateLimitQueueLimit*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_refresh_token_rate_limit_cooldown_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.RefreshTokenRateLimitCooldownSeconds = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*RefreshTokenRateLimitCooldownSeconds*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_sensitive_admin_rate_limit_permit_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.SensitiveAdminRateLimitPermitLimit = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*SensitiveAdminRateLimitPermitLimit*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_sensitive_admin_rate_limit_window_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.SensitiveAdminRateLimitWindowSeconds = 0;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*SensitiveAdminRateLimitWindowSeconds*greater than 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_sensitive_admin_rate_limit_queue_limit()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.SensitiveAdminRateLimitQueueLimit = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*SensitiveAdminRateLimitQueueLimit*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_sensitive_admin_rate_limit_cooldown_seconds()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.SensitiveAdminRateLimitCooldownSeconds = -1;

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*SensitiveAdminRateLimitCooldownSeconds*greater than or equal to 0*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_invalid_allowed_ip_range()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.AllowedIpRanges = new[] { "not-an-ip" };

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*IP range*invalid*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_conflicting_ip_ranges()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();
        request.AllowedIpRanges = new[] { "192.168.1.1" };
        request.BlockedIpRanges = new[] { "192.168.1.1" };

        var act = () => fixture.Service.UpdateAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*AllowedIpRanges and BlockedIpRanges must not contain the same value*");

        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_creates_settings_when_database_settings_do_not_exist()
    {
        var fixture = Fixture(databaseSettings: null);
        var request = ValidRequest();

        var response = await fixture.Service.UpdateAsync(request);

        response.Id.Should().Be(SecuritySettings.SingletonId);
        response.IsMfaEnabled.Should().Be(request.IsMfaEnabled);
        response.IsOtpEnabled.Should().Be(request.IsOtpEnabled);
        response.IsEmailOtpEnabled.Should().Be(request.IsEmailOtpEnabled);
        response.IsPhoneOtpEnabled.Should().Be(request.IsPhoneOtpEnabled);
        response.OtpExpirationMinutes.Should().Be(request.OtpExpirationMinutes);
        response.OtpMaxAttempts.Should().Be(request.OtpMaxAttempts);

        response.LoginRateLimitPermitLimit.Should().Be(request.LoginRateLimitPermitLimit);
        response.LoginRateLimitWindowSeconds.Should().Be(request.LoginRateLimitWindowSeconds);
        response.LoginRateLimitQueueLimit.Should().Be(request.LoginRateLimitQueueLimit);
        response.LoginRateLimitCooldownSeconds.Should().Be(request.LoginRateLimitCooldownSeconds);
        response.LoginLockoutFailureThreshold.Should().Be(request.LoginLockoutFailureThreshold);
        response.LoginLockoutDurationSeconds.Should().Be(request.LoginLockoutDurationSeconds);

        response.RefreshTokenRateLimitPermitLimit.Should().Be(request.RefreshTokenRateLimitPermitLimit);
        response.RefreshTokenRateLimitWindowSeconds.Should().Be(request.RefreshTokenRateLimitWindowSeconds);
        response.RefreshTokenRateLimitQueueLimit.Should().Be(request.RefreshTokenRateLimitQueueLimit);
        response.RefreshTokenRateLimitCooldownSeconds.Should().Be(request.RefreshTokenRateLimitCooldownSeconds);

        response.SensitiveAdminRateLimitPermitLimit.Should().Be(request.SensitiveAdminRateLimitPermitLimit);
        response.SensitiveAdminRateLimitWindowSeconds.Should().Be(request.SensitiveAdminRateLimitWindowSeconds);
        response.SensitiveAdminRateLimitQueueLimit.Should().Be(request.SensitiveAdminRateLimitQueueLimit);
        response.SensitiveAdminRateLimitCooldownSeconds.Should().Be(request.SensitiveAdminRateLimitCooldownSeconds);

        response.AllowedIpRanges.Should().Equal(request.AllowedIpRanges);
        response.BlockedIpRanges.Should().Equal(request.BlockedIpRanges);

        fixture.SecuritySettingsRepository.Verify(x => x.Add(It.IsAny<SecuritySettings>()), Times.Once);
        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_settings_when_database_settings_exist()
    {
        var existing = CreateSettings(
            isMfaEnabled: false,
            isOtpEnabled: true,
            otpExpirationMinutes: 5,
            otpMaxAttempts: 5,
            loginRateLimitPermitLimit: 10,
            loginRateLimitWindowSeconds: 60,
            refreshTokenRateLimitPermitLimit: 10,
            refreshTokenRateLimitWindowSeconds: 60,
            allowedIpRanges: Array.Empty<string>(),
            blockedIpRanges: Array.Empty<string>());

        var fixture = Fixture(existing);

        var request = new UpdateSecuritySettingsRequest
        {
            IsMfaEnabled = true,
            IsOtpEnabled = false,
            IsEmailOtpEnabled = true,
            IsPhoneOtpEnabled = false,
            OtpExpirationMinutes = 15,
            OtpMaxAttempts = 4,
            LoginRateLimitPermitLimit = 6,
            LoginRateLimitWindowSeconds = 90,
            LoginRateLimitQueueLimit = 1,
            LoginRateLimitCooldownSeconds = 3,
            LoginLockoutFailureThreshold = 7,
            LoginLockoutDurationSeconds = 600,
            RefreshTokenRateLimitPermitLimit = 12,
            RefreshTokenRateLimitWindowSeconds = 120,
            RefreshTokenRateLimitQueueLimit = 2,
            RefreshTokenRateLimitCooldownSeconds = 4,
            SensitiveAdminRateLimitPermitLimit = 40,
            SensitiveAdminRateLimitWindowSeconds = 180,
            SensitiveAdminRateLimitQueueLimit = 1,
            SensitiveAdminRateLimitCooldownSeconds = 5,
            AllowedIpRanges = new[] { "192.168.10.0/24" },
            BlockedIpRanges = new[] { "10.10.10.10" }
        };

        var response = await fixture.Service.UpdateAsync(request);

        response.IsMfaEnabled.Should().BeTrue();
        response.IsOtpEnabled.Should().BeFalse();
        response.IsEmailOtpEnabled.Should().BeTrue();
        response.IsPhoneOtpEnabled.Should().BeFalse();
        response.OtpExpirationMinutes.Should().Be(15);
        response.OtpMaxAttempts.Should().Be(4);

        response.LoginRateLimitPermitLimit.Should().Be(6);
        response.LoginRateLimitWindowSeconds.Should().Be(90);
        response.LoginRateLimitQueueLimit.Should().Be(1);
        response.LoginRateLimitCooldownSeconds.Should().Be(3);
        response.LoginLockoutFailureThreshold.Should().Be(7);
        response.LoginLockoutDurationSeconds.Should().Be(600);

        response.RefreshTokenRateLimitPermitLimit.Should().Be(12);
        response.RefreshTokenRateLimitWindowSeconds.Should().Be(120);
        response.RefreshTokenRateLimitQueueLimit.Should().Be(2);
        response.RefreshTokenRateLimitCooldownSeconds.Should().Be(4);

        response.SensitiveAdminRateLimitPermitLimit.Should().Be(40);
        response.SensitiveAdminRateLimitWindowSeconds.Should().Be(180);
        response.SensitiveAdminRateLimitQueueLimit.Should().Be(1);
        response.SensitiveAdminRateLimitCooldownSeconds.Should().Be(5);

        response.AllowedIpRanges.Should().Equal("192.168.10.0/24");
        response.BlockedIpRanges.Should().Equal("10.10.10.10");

        existing.UpdatedAt.Should().Be(Now);
        existing.UpdatedByUserId.Should().Be(ActorUserId);

        fixture.SecuritySettingsRepository.Verify(x => x.Add(It.IsAny<SecuritySettings>()), Times.Never);
        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static UpdateSecuritySettingsRequest ValidRequest() => new()
    {
        IsMfaEnabled = true,
        IsOtpEnabled = true,
        IsEmailOtpEnabled = true,
        IsPhoneOtpEnabled = true,
        OtpExpirationMinutes = 5,
        OtpMaxAttempts = 5,
        LoginRateLimitPermitLimit = 10,
        LoginRateLimitWindowSeconds = 60,
        LoginRateLimitQueueLimit = 0,
        LoginRateLimitCooldownSeconds = 2,
        LoginLockoutFailureThreshold = 5,
        LoginLockoutDurationSeconds = 900,
        RefreshTokenRateLimitPermitLimit = 10,
        RefreshTokenRateLimitWindowSeconds = 60,
        RefreshTokenRateLimitQueueLimit = 0,
        RefreshTokenRateLimitCooldownSeconds = 2,
        SensitiveAdminRateLimitPermitLimit = 30,
        SensitiveAdminRateLimitWindowSeconds = 60,
        SensitiveAdminRateLimitQueueLimit = 0,
        SensitiveAdminRateLimitCooldownSeconds = 2,
        AllowedIpRanges = new[] { "192.168.1.0/24" },
        BlockedIpRanges = new[] { "10.0.0.1" }
    };

    private static SecuritySettings CreateSettings(
        bool isMfaEnabled,
        bool isOtpEnabled,
        int otpExpirationMinutes,
        int otpMaxAttempts,
        int loginRateLimitPermitLimit,
        int loginRateLimitWindowSeconds,
        int refreshTokenRateLimitPermitLimit,
        int refreshTokenRateLimitWindowSeconds,
        string[] allowedIpRanges,
        string[] blockedIpRanges,
        bool isEmailOtpEnabled = true,
        bool isPhoneOtpEnabled = true)
    {
        return SecuritySettings.Create(
            isMfaEnabled: isMfaEnabled,
            isOtpEnabled: isOtpEnabled,
            isEmailOtpEnabled: isEmailOtpEnabled,
            isPhoneOtpEnabled: isPhoneOtpEnabled,
            otpExpirationMinutes: otpExpirationMinutes,
            otpMaxAttempts: otpMaxAttempts,
            loginRateLimitPermitLimit: loginRateLimitPermitLimit,
            loginRateLimitWindowSeconds: loginRateLimitWindowSeconds,
            loginRateLimitQueueLimit: 0,
            loginRateLimitCooldownSeconds: 2,
            loginLockoutFailureThreshold: 5,
            loginLockoutDurationSeconds: 900,
            refreshTokenRateLimitPermitLimit: refreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds: refreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit: 0,
            refreshTokenRateLimitCooldownSeconds: 2,
            sensitiveAdminRateLimitPermitLimit: 30,
            sensitiveAdminRateLimitWindowSeconds: 60,
            sensitiveAdminRateLimitQueueLimit: 0,
            sensitiveAdminRateLimitCooldownSeconds: 2,
            allowedIpRanges: allowedIpRanges,
            blockedIpRanges: blockedIpRanges,
            nowUtc: DateTimeOffset.UnixEpoch,
            actorUserId: null);
    }

    private static TestFixture Fixture(SecuritySettings? databaseSettings)
    {
        var repository = new Mock<ISecuritySettingsRepository>();
        repository
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(databaseSettings);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.SecuritySettings).Returns(repository.Object);
        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new SecuritySettingsService(
            unitOfWork.Object,
            new TestClock(),
            new TestCurrentUser(),
            Microsoft.Extensions.Options.Options.Create(new SecuritySettingsDefaultsOptions()));

        return new TestFixture(service, unitOfWork, repository);
    }

    private sealed record TestFixture(
        SecuritySettingsService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<ISecuritySettingsRepository> SecuritySettingsRepository);

    private sealed class TestClock : IClock
    {
        public DateTimeOffset Utcnow => Now;
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId => ActorUserId;
        public bool IsAuthenticated => true;
    }
}