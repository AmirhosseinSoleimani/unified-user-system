using UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IApplicationMetadataService
{
    Task<AppBootstrapResponse> GetBootstrapAsync(AppBootstrapRequest request, CancellationToken ct = default);
}
