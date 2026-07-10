using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.UnitTests.Security;

public sealed class SecurityRateLimitServiceTests
{
    private DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task First_request_within_window_is_allowed()
    {
        var store = new InMemoryDistributedRateLimitStore(() => _now);
        var service = CreateService(store, permitLimit: 2, windowSeconds: 60);

        var decision = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        decision.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Request_over_permit_limit_is_rejected_with_retry_after()
    {
        var store = new InMemoryDistributedRateLimitStore(() => _now);
        var service = CreateService(store, permitLimit: 1, windowSeconds: 60);

        await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));
        var rejected = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        rejected.IsAllowed.Should().BeFalse();
        rejected.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task Counter_resets_after_window_expires()
    {
        var store = new InMemoryDistributedRateLimitStore(() => _now);
        var service = CreateService(store, permitLimit: 1, windowSeconds: 10);

        await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));
        var rejected = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        _now = _now.AddSeconds(11);
        var allowed = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        rejected.IsAllowed.Should().BeFalse();
        allowed.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Redis_unavailable_fails_closed_by_default()
    {
        var store = new InMemoryDistributedRateLimitStore(() => _now)
        {
            IsUnavailable = true
        };

        var service = CreateService(store, permitLimit: 1, windowSeconds: 60);

        var decision = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Contain("unavailable");
    }

    [Fact]
    public async Task Admin_settings_control_permit_and_window()
    {
        var store = new InMemoryDistributedRateLimitStore(() => _now);
        var service = CreateService(store, permitLimit: 3, windowSeconds: 5);

        (await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"))).IsAllowed.Should().BeTrue();
        (await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"))).IsAllowed.Should().BeTrue();
        (await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"))).IsAllowed.Should().BeTrue();

        var rejected = await service.CheckAsync(new SecurityRateLimitContext("Auth", "client-1"));

        rejected.IsAllowed.Should().BeFalse();
        rejected.RetryAfter.Should().BeCloseTo(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
    }

    private static SecurityRateLimitService CreateService(
        InMemoryDistributedRateLimitStore store,
        int permitLimit,
        int windowSeconds)
    {
        var settings = new SecuritySettingsResponse
        {
            LoginRateLimitPermitLimit = permitLimit,
            LoginRateLimitWindowSeconds = windowSeconds,
            LoginRateLimitQueueLimit = 0,
            LoginRateLimitCooldownSeconds = 2,
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

        return new SecurityRateLimitService(
            store,
            settingsService.Object,
            Options.Create(new SecurityRuntimeOptions()));
    }
}
