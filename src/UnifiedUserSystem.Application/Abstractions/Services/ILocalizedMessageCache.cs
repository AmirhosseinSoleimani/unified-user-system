using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface ILocalizedMessageCache
{
    Task<LocalizedMessageCacheSnapshot> GetSnapshotAsync(CancellationToken ct = default);
    Task<LocalizedMessage> GetAsync(string key, LocalizedMessage fallback, CancellationToken ct = default);
    Task RefreshAsync(CancellationToken ct = default);
}