using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Application.Services.Security;

public sealed class SecurityRateLimitService : ISecurityRateLimitService
{
    private readonly IDistributedRateLimitStore _store;
    private readonly ISecuritySettingsService _securitySettingsService;
    private readonly SecurityRuntimeOptions _runtimeOptions;

    public SecurityRateLimitService(
        IDistributedRateLimitStore store,
        ISecuritySettingsService securitySettingsService,
        IOptions<SecurityRuntimeOptions> runtimeOptions)
    {
        _store = store;
        _securitySettingsService = securitySettingsService;
        _runtimeOptions = runtimeOptions.Value;
    }

    public async Task<RateLimitDecision> CheckAsync(
        SecurityRateLimitContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = await _securitySettingsService.GetEffectiveAsync(cancellationToken);
        var policy = ResolvePolicy(context.PolicyName, settings);

        var key = $"{Normalize(_runtimeOptions.RateLimitKeyPrefix)}:rate-limit:{Normalize(context.PolicyName)}:{Normalize(context.PartitionKey)}";

        var lease = await _store.TryAcquireAsync(
            key,
            policy.PermitLimit,
            TimeSpan.FromSeconds(policy.WindowSeconds),
            policy.CooldownSeconds > 0 ? TimeSpan.FromSeconds(policy.CooldownSeconds) : null,
            cancellationToken);

        if (lease.IsUnavailable)
        {
            return IsFailOpen()
                ? RateLimitDecision.Allowed()
                : RateLimitDecision.Rejected(null, "Rate limit storage is unavailable.");
        }

        return lease.IsAcquired
            ? RateLimitDecision.Allowed()
            : RateLimitDecision.Rejected(lease.RetryAfter, lease.Reason ?? "Too many requests.");
    }

    private bool IsFailOpen()
        => string.Equals(_runtimeOptions.RedisUnavailableMode, "FailOpen", StringComparison.OrdinalIgnoreCase);

    private static SecurityRateLimitPolicy ResolvePolicy(string policyName, SecuritySettingsResponse settings)
    {
        if (string.Equals(policyName, "RefreshToken", StringComparison.OrdinalIgnoreCase))
        {
            return new SecurityRateLimitPolicy(
                settings.RefreshTokenRateLimitPermitLimit,
                settings.RefreshTokenRateLimitWindowSeconds,
                settings.RefreshTokenRateLimitQueueLimit,
                settings.RefreshTokenRateLimitCooldownSeconds);
        }

        if (string.Equals(policyName, "SensitiveAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return new SecurityRateLimitPolicy(
                settings.SensitiveAdminRateLimitPermitLimit,
                settings.SensitiveAdminRateLimitWindowSeconds,
                settings.SensitiveAdminRateLimitQueueLimit,
                settings.SensitiveAdminRateLimitCooldownSeconds);
        }

        return new SecurityRateLimitPolicy(
            settings.LoginRateLimitPermitLimit,
            settings.LoginRateLimitWindowSeconds,
            settings.LoginRateLimitQueueLimit,
            settings.LoginRateLimitCooldownSeconds);
    }

    private static string Normalize(string value)
        => string.IsNullOrWhiteSpace(value)
            ? "unknown"
            : value.Trim().ToLowerInvariant();

    private sealed record SecurityRateLimitPolicy(
        int PermitLimit,
        int WindowSeconds,
        int QueueLimit,
        int CooldownSeconds);
}
