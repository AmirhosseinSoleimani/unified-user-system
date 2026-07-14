using UnifiedUserSystem.src.Contracts.DTOs.Localization;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IErrorMessageService
{
    Task<IReadOnlyList<ErrorMessageResponse>> ListAsync(CancellationToken ct = default);
    Task<ErrorMessageResponse> UpsertAsync(UpsertErrorMessageRequest request, CancellationToken ct = default);
    Task RefreshCacheAsync(CancellationToken ct = default);
}
