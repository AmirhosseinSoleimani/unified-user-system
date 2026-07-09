using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;
using UnifiedUserSystem.src.Application.Abstractions.Auditing;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Infrastructure.Persistence;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Authorization;
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
            .AddSecurity()
            .AddWebContext()
            .AddSystemServices();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IRoleOperationRepository, RoleOperationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();
        services.AddScoped<IPermissionReadRepository, EfPermissionReadRepository>();
        services.AddScoped<ISecuritySettingsRepository, SecuritySettingsRepository>();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddSingleton<ITemporarySecurityStateStore, MemoryTemporarySecurityStateStore>();
        services.AddSingleton<IPermissionCache, MemoryPermissionCache>();

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