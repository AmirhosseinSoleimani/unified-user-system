using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Api.Integration
{
    public sealed class AuthApiFactory : WebApplicationFactory<Program>
    {
        private const string JwtKey = "test-signing-key-that-is-long-enough-for-hmac-sha256-and-integration-tests";
        private const string JwtIssuer = "UnifiedUserSystem.Tests";
        private const string JwtAudience = "UnifiedUserSystem.Tests.Client";

        private readonly string _databaseName = $"auth-api-tests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = "Host=localhost;Database=test",
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["Jwt:ExpiresMinutes"] = "15",
                    ["RefreshToken:ExpiresDays"] = "7",
                    ["RefreshToken:TokenSizeBytes"] = "64"
                });
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
            });
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
    }
}