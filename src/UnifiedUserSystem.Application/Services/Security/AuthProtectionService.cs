using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.src.Application.Services.Security
{
    public sealed class AuthProtectionService : IAuthProtectionService
    {
        private const string Prefix = "auth-protection:login";

        private readonly ITemporarySecurityStateStore _store;
        private readonly ISecuritySettingsService _securitySettingsService;

        public AuthProtectionService(
            ITemporarySecurityStateStore store,
            ISecuritySettingsService securitySettingsService)
        {
            _store = store;
            _securitySettingsService = securitySettingsService;
        }

        public async Task<AuthProtectionCheckResult> CheckAsync(
            string normalizedLoginIdentifier,
            IClientContext clientContext,
            CancellationToken ct = default)
        {
            try
            {
                var identityLockout = await _store.GetStringAsync(IdentityLockoutKey(normalizedLoginIdentifier), ct);
                if (identityLockout is not null)
                    return AuthProtectionCheckResult.Block(reason: "Login identifier is locked out.");

                var clientLockout = await _store.GetStringAsync(ClientLockoutKey(clientContext), ct);
                if (clientLockout is not null)
                    return AuthProtectionCheckResult.Block(reason: "Client is locked out.");

                var cooldown = await _store.GetStringAsync(ClientCooldownKey(clientContext), ct);
                if (cooldown is not null)
                    return AuthProtectionCheckResult.Block(reason: "Login cooldown is active.");

                return AuthProtectionCheckResult.Allow();
            }
            catch (SecurityStateUnavailableException)
            {
                return AuthProtectionCheckResult.Block(reason: "Authentication protection state is unavailable.");
            }
        }

        public async Task RecordFailureAsync(
        string normalizedLoginIdentifier,
        IClientContext clientContext,
        CancellationToken ct = default)
        {
            var settings = await _securitySettingsService.GetEffectiveAsync(ct);

            var window = TimeSpan.FromSeconds(settings.LoginRateLimitWindowSeconds);
            var lockout = TimeSpan.FromSeconds(settings.LoginLockoutDurationSeconds);
            var cooldown = TimeSpan.FromSeconds(settings.LoginRateLimitCooldownSeconds);
            var identityThreshold = settings.LoginLockoutFailureThreshold;
            var clientThreshold = Math.Max(identityThreshold * 4, identityThreshold);

            var identityFailures = await _store.IncrementAsync(IdentityFailuresKey(normalizedLoginIdentifier), window, ct);
            var clientFailures = await _store.IncrementAsync(ClientFailuresKey(clientContext), window, ct);

            if (identityFailures >= identityThreshold)
                await _store.SetStringAsync(IdentityLockoutKey(normalizedLoginIdentifier), "1", lockout, ct);

            if (clientFailures >= clientThreshold)
                await _store.SetStringAsync(ClientLockoutKey(clientContext), "1", lockout, ct);

            if (cooldown > TimeSpan.Zero)
                await _store.SetStringAsync(ClientCooldownKey(clientContext), "1", cooldown, ct);
        }

        public async Task ResetAsync(
        string normalizedLoginIdentifier,
        IClientContext clientContext,
        CancellationToken ct = default)
        {
            await _store.RemoveAsync(IdentityFailuresKey(normalizedLoginIdentifier), ct);
            await _store.RemoveAsync(IdentityLockoutKey(normalizedLoginIdentifier), ct);
            await _store.RemoveAsync(ClientFailuresKey(clientContext), ct);
            await _store.RemoveAsync(ClientLockoutKey(clientContext), ct);
            await _store.RemoveAsync(ClientCooldownKey(clientContext), ct);
        }

        private static string IdentityFailuresKey(string normalizedLoginIdentifier)
        => $"{Prefix}:identity:{Hash(NormalizeIdentifier(normalizedLoginIdentifier))}:failures";

        private static string IdentityLockoutKey(string normalizedLoginIdentifier)
            => $"{Prefix}:identity:{Hash(NormalizeIdentifier(normalizedLoginIdentifier))}:lockout";

        private static string ClientFailuresKey(IClientContext clientContext)
            => $"{Prefix}:client:{Hash(BuildClientKey(clientContext))}:failures";

        private static string ClientLockoutKey(IClientContext clientContext)
            => $"{Prefix}:client:{Hash(BuildClientKey(clientContext))}:lockout";

        private static string ClientCooldownKey(IClientContext clientContext)
            => $"{Prefix}:client:{Hash(BuildClientKey(clientContext))}:cooldown";

        private static string NormalizeIdentifier(string value)
            => (value ?? string.Empty).Trim().ToLowerInvariant();

        private static string BuildClientKey(IClientContext clientContext)
        {
            var ipAddress = NormalizeClientPart(clientContext.IpAddress) ?? "unknown-ip";
            var clientId = NormalizeClientPart(clientContext.ClientId);

            return clientId is null
                ? ipAddress
                : $"{clientId}:{ipAddress}";
        }

        private static string? NormalizeClientPart(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

        private static string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}