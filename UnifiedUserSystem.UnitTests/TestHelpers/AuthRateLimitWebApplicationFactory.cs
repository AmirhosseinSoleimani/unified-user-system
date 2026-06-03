using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.TestHelpers
{
    public sealed class AuthRateLimitWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly bool _disableSecurityRateLimiting;
        private readonly bool _bypassAuthorization;
        private readonly int _authPermitLimit;
        private readonly int _sensitiveAdminPermitLimit;
        private readonly int _maxFailedAttemptsPerIdentity;
        private readonly int _maxFailedAttemptsPerClient;
        private readonly int _cooldownSeconds;

        public AuthRateLimitWebApplicationFactory(
            bool disableSecurityRateLimiting = false,
            bool bypassAuthorization = true,
            int authPermitLimit = 2,
            int sensitiveAdminPermitLimit = 2,
            int maxFailedAttemptsPerIdentity = 50,
            int maxFailedAttemptsPerClient = 500,
            int cooldownSeconds = 0)
        {
            _disableSecurityRateLimiting = disableSecurityRateLimiting;
            _bypassAuthorization = bypassAuthorization;
            _authPermitLimit = authPermitLimit;
            _sensitiveAdminPermitLimit = sensitiveAdminPermitLimit;
            _maxFailedAttemptsPerIdentity = maxFailedAttemptsPerIdentity;
            _maxFailedAttemptsPerClient = maxFailedAttemptsPerClient;
            _cooldownSeconds = cooldownSeconds;
        }

        public MutableTestClock Clock { get; } =
            new(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var values = new Dictionary<string, string?>
                {
                    ["DisableSecurityRateLimiting"] = _disableSecurityRateLimiting.ToString(),

                    ["ConnectionStrings:Default"] = "Host=localhost;Database=test",

                    ["Jwt:Key"] = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                    ["Jwt:Issuer"] = "UnifiedUserSystem",
                    ["Jwt:Audience"] = "UnifiedUserSystemClient",
                    ["Jwt:ExpiresMinutes"] = "15",

                    ["RefreshToken:ExpiresDays"] = "7",
                    ["RefreshToken:TokenSizeBytes"] = "64",

                    ["AuthProtection:MaxFailedAttemptsPerIdentity"] = _maxFailedAttemptsPerIdentity.ToString(),
                    ["AuthProtection:MaxFailedAttemptsPerClient"] = _maxFailedAttemptsPerClient.ToString(),
                    ["AuthProtection:FailedAttemptWindowMinutes"] = "1",
                    ["AuthProtection:LockoutMinutes"] = "1",
                    ["AuthProtection:CooldownSeconds"] = _cooldownSeconds.ToString(),

                    ["AuthProtection:AuthRateLimitPermitLimit"] = _authPermitLimit.ToString(),
                    ["AuthProtection:AuthRateLimitWindowSeconds"] = "60",
                    ["AuthProtection:AuthRateLimitQueueLimit"] = "0",

                    ["AuthProtection:SensitiveAdminRateLimitPermitLimit"] = _sensitiveAdminPermitLimit.ToString(),
                    ["AuthProtection:SensitiveAdminRateLimitWindowSeconds"] = "60",
                    ["AuthProtection:SensitiveAdminRateLimitQueueLimit"] = "0",

                    ["SecurityRateLimits:Auth:PermitLimit"] = _authPermitLimit.ToString(),
                    ["SecurityRateLimits:Auth:WindowSeconds"] = "60",
                    ["SecurityRateLimits:SensitiveAdmin:PermitLimit"] = _sensitiveAdminPermitLimit.ToString(),
                    ["SecurityRateLimits:SensitiveAdmin:WindowSeconds"] = "60"
                };

                config.AddInMemoryCollection(values);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IClock>();
                services.RemoveAll<ITemporarySecurityStateStore>();

                services.AddSingleton<IClock>(Clock);
                services.AddSingleton<ITemporarySecurityStateStore, MemoryTemporarySecurityStateStore>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase($"unified-user-system-tests-{Guid.NewGuid()}"));

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });

                if (_bypassAuthorization)
                {
                    services.RemoveAll<IAuthorizationHandler>();
                    services.AddSingleton<IAuthorizationHandler, TestAuthorizationHandler>();
                }
            });
        }

        public HttpClient CreateAnonymousClient(string? clientId = null)
        {
            var client = CreateClient();

            client.DefaultRequestHeaders.Add("X-Client-Id", clientId ?? Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

            return client;
        }

        public HttpClient CreateAuthenticatedClient(string? clientId = null, Guid? userId = null)
        {
            var client = CreateAnonymousClient(clientId);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(TestAuthHandler.SchemeName);

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                (userId ?? Guid.NewGuid()).ToString());

            return client;
        }
    }
}