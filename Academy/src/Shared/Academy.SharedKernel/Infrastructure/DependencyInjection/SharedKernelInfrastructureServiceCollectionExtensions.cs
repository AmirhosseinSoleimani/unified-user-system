using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Persistence.Interceptors;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Security;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Serialization;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Time;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.DependencyInjection;

public static class SharedKernelInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICurrentUserContext, SystemCurrentUserContext>();
        services.AddSingleton<IEventSerializer, SystemTextJsonEventSerializer>();
        services.AddScoped<OutboxMessageFactory>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        return services;
    }
}