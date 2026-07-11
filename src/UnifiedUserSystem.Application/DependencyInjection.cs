using Microsoft.Extensions.DependencyInjection;
using UnifiedUserSystem.Application.Abstractions.Services;
using UnifiedUserSystem.Application.Services.Authentication;
using UnifiedUserSystem.Application.Services.Authorization;
using UnifiedUserSystem.src.Application.Abstractions.Auditing;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Application.Validation;

namespace UnifiedUserSystem.src.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddApplicationValidation()
            .AddApplicationSecurity()
            .AddApplicationAuditing()
            .AddApplicationUseCases();

        return services;
    }

    private static IServiceCollection AddApplicationValidation(this IServiceCollection services)
    {
        services.AddScoped<IPasswordPolicy, PasswordPolicy>();
        services.AddScoped<IRegistrationRequestValidator, RegistrationRequestValidator>();
        services.AddScoped<ILoginRequestValidator, LoginRequestValidator>();

        return services;
    }

    private static IServiceCollection AddApplicationSecurity(this IServiceCollection services)
    {
        services.AddScoped<IAuthProtectionService, AuthProtectionService>();
        services.AddScoped<ISecurityRateLimitService, SecurityRateLimitService>();
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
        services.AddScoped<IPermissionCacheInvalidator, PermissionCacheInvalidator>();
        services.AddScoped<ISecuritySettingsService, SecuritySettingsService>();
        services.AddScoped<IIpManagementService, IpManagementService>();
        services.AddScoped<IIpAccessPolicyService, IpAccessPolicyService>();
        services.AddScoped<IIpSecurityReportService, IpSecurityReportService>();

        return services;
    }

    private static IServiceCollection AddApplicationAuditing(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();

        return services;
    }

    private static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}