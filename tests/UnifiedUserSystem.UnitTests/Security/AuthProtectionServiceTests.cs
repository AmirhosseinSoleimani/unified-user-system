using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.UnitTests.Security;

public sealed class AuthProtectionServiceTests
{
    private DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task Failed_login_increments_failure_counter_and_creates_lockout()
    {
        var store = new InMemoryTemporarySecurityStateStore(() => _now);
        var service = CreateService(store, threshold: 2, lockoutSeconds: 60);

        await service.RecordFailureAsync("user@example.com", ClientContext(), CancellationToken.None);
        var beforeThreshold = await service.CheckAsync("user@example.com", ClientContext(), CancellationToken.None);

        await service.RecordFailureAsync("user@example.com", ClientContext(), CancellationToken.None);
        var locked = await service.CheckAsync("user@example.com", ClientContext(), CancellationToken.None);

        Assert.False(beforeThreshold.IsAllowed);
        Assert.False(locked.IsAllowed);
        Assert.Contains("locked", locked.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Reset_removes_failed_login_counter_and_lockout()
    {
        var store = new InMemoryTemporarySecurityStateStore(() => _now);
        var service = CreateService(store, threshold: 1, lockoutSeconds: 60);

        await service.RecordFailureAsync("user@example.com", ClientContext(), CancellationToken.None);
        var locked = await service.CheckAsync("user@example.com", ClientContext(), CancellationToken.None);

        await service.ResetAsync("user@example.com", ClientContext(), CancellationToken.None);
        var allowed = await service.CheckAsync("user@example.com", ClientContext(), CancellationToken.None);

        Assert.False(locked.IsAllowed);
        Assert.True(allowed.IsAllowed);
    }

    [Fact]
    public async Task Redis_unavailable_blocks_auth_protection_path()
    {
        var store = new InMemoryTemporarySecurityStateStore(() => _now)
        {
            IsUnavailable = true
        };

        var service = CreateService(store, threshold: 5, lockoutSeconds: 60);

        var result = await service.CheckAsync("user@example.com", ClientContext(), CancellationToken.None);

        Assert.False(result.IsAllowed);
        Assert.Contains("unavailable", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    private static AuthProtectionService CreateService(
        InMemoryTemporarySecurityStateStore store,
        int threshold,
        int lockoutSeconds)
    {
        var settings = new SecuritySettingsResponse
        {
            LoginRateLimitPermitLimit = 10,
            LoginRateLimitWindowSeconds = 60,
            LoginRateLimitQueueLimit = 0,
            LoginRateLimitCooldownSeconds = 2,
            LoginLockoutFailureThreshold = threshold,
            LoginLockoutDurationSeconds = lockoutSeconds,
            RefreshTokenRateLimitPermitLimit = 10,
            RefreshTokenRateLimitWindowSeconds = 60,
            RefreshTokenRateLimitQueueLimit = 0,
            RefreshTokenRateLimitCooldownSeconds = 2,
            SensitiveAdminRateLimitPermitLimit = 30,
            SensitiveAdminRateLimitWindowSeconds = 60,
            SensitiveAdminRateLimitQueueLimit = 0,
            SensitiveAdminRateLimitCooldownSeconds = 2
        };

        var settingsService = new Mock<ISecuritySettingsService>();
        settingsService
            .Setup(x => x.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        return new AuthProtectionService(store, settingsService.Object);
    }

    private static IClientContext ClientContext()
    {
        var context = new Mock<IClientContext>();
        context.SetupGet(x => x.IpAddress).Returns("127.0.0.1");
        context.SetupGet(x => x.ClientId).Returns("test-client");
        context.SetupGet(x => x.UserAgent).Returns("tests");

        return context.Object;
    }
}