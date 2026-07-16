using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Api.Options;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApiControllers()
            .AddApiAuthorization()
            .AddApiSwagger()
            .AddApiLocalization(configuration)
            .AddApiMiddlewares();

        return services;

    }


    private static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(ApiModelValidationOptions.Configure);

        return services;
    }

    private static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, OperationAuthorizationHandler>();

        return services;
    }

    private static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UnifiedUserSystem API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddApiLocalization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<LocalizationOptions>()
            .Bind(
                configuration.GetSection(
                    LocalizationOptions.SectionName))
            .Validate(
                options => options.IsValid(),
                "Localization configuration is invalid.")
            .ValidateOnStart();

        services.AddScoped<
            IRequestLocaleResolver,
            RequestLocaleResolver>();

        services.AddSingleton<
            IBusinessMessageLocalizer,
            DictionaryBusinessMessageLocalizer>();

        services.AddHttpClient<
            IGeoIpCountryResolver,
            HttpGeoIpCountryResolver>(
            (serviceProvider, httpClient) =>
            {
                var localizationOptions = serviceProvider
                    .GetRequiredService<
                        IOptions<LocalizationOptions>>()
                    .Value;

                var timeoutSeconds = Math.Clamp(
                    localizationOptions.GeoIp.TimeoutSeconds,
                    1,
                    10);

                httpClient.Timeout =
                    TimeSpan.FromSeconds(timeoutSeconds);

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "UnifiedUserSystem-GeoIp/1.0");
            });

        return services;
    }

    private static IServiceCollection AddApiMiddlewares(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddScoped<SecurityRateLimitingMiddleware>();
        services.AddScoped<IpAccessControlMiddleware>();
        services.AddSingleton<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersOptionsSetup>();

        return services;
    }

}