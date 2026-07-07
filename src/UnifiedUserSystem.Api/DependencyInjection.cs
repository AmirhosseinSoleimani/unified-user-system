using Microsoft.AspNetCore.Authorization;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Api.Middlewares;

namespace UnifiedUserSystem.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddAuthorization();

        services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, OperationAuthorizationHandler>();
        services.AddScoped<ExceptionHandlingMiddleware>();

        return services;
    }
}