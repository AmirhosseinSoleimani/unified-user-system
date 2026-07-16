using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UnifiedUserSystem.src.Application.Abstractions.Auditing;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Infrastructure.Persistence;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Authorization;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.Infrastructure.Web;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Security;

namespace UnifiedUserSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddSecurity(configuration)
            .AddWebContext()
            .AddSystemServices();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is missing.")));


        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IRoleOperationRepository, RoleOperationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();
        services.AddScoped<IPermissionReadRepository, EfPermissionReadRepository>();
        services.AddScoped<ISecuritySettingsRepository, SecuritySettingsRepository>();
        services.AddScoped<IIpRuleRepository, IpRuleRepository>();
        services.AddScoped<IIpSecurityEventRepository, IpSecurityEventRepository>();
        services.AddScoped<IMfaChallengeRepository, MfaChallengeRepository>();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection("Redis"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis:ConnectionString is required.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var redisOptions = serviceProvider
                .GetRequiredService<IOptions<RedisOptions>>()
                .Value;

            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Redis");

            var configurationOptions = ConfigurationOptions.Parse(
                redisOptions.ConnectionString);

            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 5;
            configurationOptions.ConnectTimeout = 5000;
            configurationOptions.SyncTimeout = 5000;
            configurationOptions.AsyncTimeout = 5000;
            configurationOptions.ClientName = "UnifiedUserSystem.Api";

            logger.LogInformation(
                "Connecting to Redis using {RedisConfiguration}",
                configurationOptions.ToString(includePassword: false));

            var connection = ConnectionMultiplexer.Connect(configurationOptions);

            connection.ConnectionFailed += (_, args) =>
            {
                logger.LogError(
                    args.Exception,
                    "Redis connection failed. Endpoint: {Endpoint}, FailureType: {FailureType}",
                    args.EndPoint,
                    args.FailureType);
            };

            connection.ConnectionRestored += (_, args) =>
            {
                logger.LogInformation(
                    "Redis connection restored. Endpoint: {Endpoint}",
                    args.EndPoint);
            };

            connection.ErrorMessage += (_, args) =>
            {
                logger.LogError(
                    "Redis server error: {RedisError}",
                    args.Message);
            };

            return connection;
        });

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<ITemporarySecurityStateStore, RedisTemporarySecurityStateStore>();
        services.AddSingleton<IDistributedRateLimitStore, RedisDistributedRateLimitStore>();
        services.AddSingleton<IPermissionCache, MemoryPermissionCache>();
        services.AddScoped<IOtpGenerator, OtpGenerator>();
        services.AddScoped<IOtpHasher, Sha256OtpHasher>();
        services.AddScoped<IEmailOtpSender, DevelopmentEmailOtpSender>();
        services.AddScoped<ISmsOtpSender, DevelopmentSmsOtpSender>();

        return services;
    }

    private static IServiceCollection AddWebContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IClientContext, ClientContext>();

        return services;
    }

    private static IServiceCollection AddSystemServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}