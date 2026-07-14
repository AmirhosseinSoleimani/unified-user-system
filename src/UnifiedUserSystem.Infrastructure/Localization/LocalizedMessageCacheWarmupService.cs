using Microsoft.Extensions.Hosting;
using UnifiedUserSystem.src.Application.Abstractions.Services;

namespace UnifiedUserSystem.src.Infrastructure.Localization;

public sealed class LocalizedMessageCacheWarmupService : IHostedService
{
    private readonly ILocalizedMessageCache _localizedMessageCache;

    public LocalizedMessageCacheWarmupService(ILocalizedMessageCache localizedMessageCache)
    {
        _localizedMessageCache = localizedMessageCache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _localizedMessageCache.RefreshAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

