
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Infrastructure.Localization;

public sealed class LocalizedMessageCache : ILocalizedMessageCache
{
    private const string CacheKey = "localized-error-messages";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly IMemoryCache _memoryCache;
    private readonly IServiceScopeFactory _scopeFactory;

    public LocalizedMessageCache(
        IMemoryCache memoryCache,
        IServiceScopeFactory scopeFactory)
    {
        _memoryCache = memoryCache;
        _scopeFactory = scopeFactory;
    }

    public async Task<LocalizedMessageCacheSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        if (_memoryCache.TryGetValue(CacheKey, out LocalizedMessageCacheSnapshot? snapshot) &&
            snapshot is not null)
        {
            return snapshot;
        }

        await RefreshAsync(ct);

        return _memoryCache.TryGetValue(CacheKey, out snapshot) && snapshot is not null
            ? snapshot
            : BuildFallbackSnapshot();
    }

    public async Task<LocalizedMessage> GetAsync(
        string key,
        LocalizedMessage fallback,
        CancellationToken ct = default)
    {
        var snapshot = await GetSnapshotAsync(ct);

        return snapshot.Messages.TryGetValue(key, out var message)
            ? message
            : fallback;
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        await RefreshLock.WaitAsync(ct);

        try
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var errorMessageRepository = scope.ServiceProvider.GetRequiredService<IErrorMessageRepository>();
                var messages = await errorMessageRepository.ListAsync(activeOnly: true, ct);
                var dictionary = messages.ToDictionary(
                    x => x.Key,
                    x => x.ToLocalizedMessage(),
                    StringComparer.OrdinalIgnoreCase);

                var version = messages.Count == 0
                    ? "empty"
                    : messages.Max(x => x.UpdatedAt).UtcDateTime.Ticks.ToString();

                SetSnapshot(new LocalizedMessageCacheSnapshot(version, dictionary));
            }
            catch
            {
                if (!_memoryCache.TryGetValue(CacheKey, out LocalizedMessageCacheSnapshot? existing) ||
                    existing is null)
                {
                    SetSnapshot(BuildFallbackSnapshot());
                }
            }
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private void SetSnapshot(LocalizedMessageCacheSnapshot snapshot)
    {
        _memoryCache.Set(
            CacheKey,
            snapshot,
            new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
    }

    private static LocalizedMessageCacheSnapshot BuildFallbackSnapshot()
    {
        var fallback = new Dictionary<string, LocalizedMessage>(StringComparer.OrdinalIgnoreCase)
        {
            ["domain_error.title"] = new("Domain error", "خطای دامنه"),
            ["domain_error.detail"] = new("Request does not satisfy domain rules.", "درخواست با قوانین دامنه سازگار نیست."),
            ["conflict.title"] = new("Conflict", "تداخل"),
            ["conflict.detail"] = new("Request conflicts with the current system state.", "درخواست با وضعیت فعلی سیستم تداخل دارد."),
            ["not_found.title"] = new("Not found", "یافت نشد"),
            ["not_found.detail"] = new("The requested resource was not found.", "منبع مورد نظر پیدا نشد."),
            ["bad_request.title"] = new("Bad request", "درخواست نامعتبر"),
            ["bad_request.detail"] = new("The submitted request is invalid.", "درخواست ارسال‌شده معتبر نیست."),
            ["server_error.title"] = new("Server error", "خطای سرور"),
            ["server_error.detail"] = new("An unexpected error occurred.", "یک خطای غیرمنتظره رخ داد.")
        };

        return new LocalizedMessageCacheSnapshot("fallback", fallback);
    }
}
