using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UnifiedUserSystem.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Api.Integration;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    private const string JwtKey = "test-signing-key-that-is-long-enough-for-hmac-sha256-and-integration-tests";
    private const string JwtIssuer = "UnifiedUserSystem.Tests";
    private const string JwtAudience = "UnifiedUserSystem.Tests.Client";

    private readonly string _databaseName = $"auth-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:ExpiresMinutes", "15");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(TestConfiguration());
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(x =>
                x.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            SeedRoles(db);
        });

        builder.ConfigureTestServices(services =>
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.Configure<JwtOptions>(options =>
            {
                options.Key = JwtKey;
                options.Issuer = JwtIssuer;
                options.Audience = JwtAudience;
                options.ExpiresMinutes = 15;
            });

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = JwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = JwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.RemoveAll<ITemporarySecurityStateStore>();
            services.AddSingleton<ITemporarySecurityStateStore, TestTemporarySecurityStateStore>();

            services.RemoveAll<IDistributedRateLimitStore>();
            services.AddSingleton<IDistributedRateLimitStore, TestDistributedRateLimitStore>();

            services.RemoveAll<IIpAccessPolicyService>();
            services.AddScoped<IIpAccessPolicyService, AllowAllIpAccessPolicyService>();
        });
    }

    private static Dictionary<string, string?> TestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=unified_user_system_tests;Username=postgres;Password=postgres",

            ["Jwt:Issuer"] = JwtIssuer,
            ["Jwt:Audience"] = JwtAudience,
            ["Jwt:Key"] = JwtKey,
            ["Jwt:ExpiresMinutes"] = "15",
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7",

            ["DisableSecurityRateLimiting"] = "true",

            ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1",
            ["ForwardedHeaders:KnownNetworks:0"] = "127.0.0.0/8",

            ["SecurityRuntime:RedisUnavailableMode"] = "FailOpen",
            ["SecurityRuntime:RateLimitKeyPrefix"] = "UnifiedUserSystemTests",

            ["Redis:ConnectionString"] = "localhost:6379,abortConnect=false",
            ["Redis:InstanceName"] = "UnifiedUserSystemTests:",

            ["SecuritySettingsDefaults:IsMfaEnabled"] = "false",
            ["SecuritySettingsDefaults:IsOtpEnabled"] = "true",
            ["SecuritySettingsDefaults:IsEmailOtpEnabled"] = "true",
            ["SecuritySettingsDefaults:IsPhoneOtpEnabled"] = "true",
            ["SecuritySettingsDefaults:OtpExpirationMinutes"] = "5",
            ["SecuritySettingsDefaults:OtpMaxAttempts"] = "5",

            ["SecuritySettingsDefaults:LoginRateLimitPermitLimit"] = "1000",
            ["SecuritySettingsDefaults:LoginRateLimitWindowSeconds"] = "60",
            ["SecuritySettingsDefaults:LoginRateLimitQueueLimit"] = "0",
            ["SecuritySettingsDefaults:LoginRateLimitCooldownSeconds"] = "0",
            ["SecuritySettingsDefaults:LoginLockoutFailureThreshold"] = "1000",
            ["SecuritySettingsDefaults:LoginLockoutDurationSeconds"] = "60",

            ["SecuritySettingsDefaults:RefreshTokenRateLimitPermitLimit"] = "1000",
            ["SecuritySettingsDefaults:RefreshTokenRateLimitWindowSeconds"] = "60",
            ["SecuritySettingsDefaults:RefreshTokenRateLimitQueueLimit"] = "0",
            ["SecuritySettingsDefaults:RefreshTokenRateLimitCooldownSeconds"] = "0",

            ["SecuritySettingsDefaults:SensitiveAdminRateLimitPermitLimit"] = "1000",
            ["SecuritySettingsDefaults:SensitiveAdminRateLimitWindowSeconds"] = "60",
            ["SecuritySettingsDefaults:SensitiveAdminRateLimitQueueLimit"] = "0",
            ["SecuritySettingsDefaults:SensitiveAdminRateLimitCooldownSeconds"] = "0"
        };
    }

    private static void SeedRoles(AppDbContext db)
    {
        if (db.Roles.Any())
            return;

        var now = new DateTimeOffset(2026, 06, 01, 10, 00, 00, TimeSpan.Zero);

        var userRole = Role.Create("user", "user", now, null);
        SetProtectedId(userRole, (int)AppRole.User);

        var adminRole = Role.Create("admin", "admin", now, null);
        SetProtectedId(adminRole, (int)AppRole.Admin);

        db.Roles.AddRange(userRole, adminRole);
        db.SaveChanges();
    }

    private static void SetProtectedId<TKey>(Entity<TKey> entity, TKey id)
        where TKey : struct
    {
        typeof(Entity<TKey>)
            .GetProperty(nameof(Entity<TKey>.Id))!
            .SetValue(entity, id);
    }

    private sealed class AllowAllIpAccessPolicyService : IIpAccessPolicyService
    {
        public Task<IpAccessDecision> CheckAsync(
            string ipAddress,
            string? path,
            string? method,
            Guid? userId,
            CancellationToken ct = default)
        {
            return Task.FromResult(IpAccessDecision.Allowed());
        }
    }

    private sealed class TestDistributedRateLimitStore : IDistributedRateLimitStore
    {
        public Task<DistributedRateLimitLeaseResult> TryAcquireAsync(
            string key,
            int permitLimit,
            TimeSpan window,
            TimeSpan? cooldown,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DistributedRateLimitLeaseResult.Acquired(1));
        }

        public Task ResetAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestTemporarySecurityStateStore : ITemporarySecurityStateStore
    {
        private readonly Dictionary<string, string> _values = new();

        public Task<string?> GetStringAsync(string key, CancellationToken ct = default)
        {
            _values.TryGetValue(key, out var value);
            return Task.FromResult<string?>(value);
        }

        public Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
        {
            _values[key] = value;
            return Task.CompletedTask;
        }

        public Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        {
            if (!_values.TryGetValue(key, out var currentValue) || !long.TryParse(currentValue, out var current))
                current = 0;

            var next = current + 1;
            _values[key] = next.ToString();

            return Task.FromResult(next);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}