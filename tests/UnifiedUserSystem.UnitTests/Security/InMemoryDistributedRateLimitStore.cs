using System.Collections.Concurrent;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.UnitTests.Security;

internal sealed class InMemoryDistributedRateLimitStore : IDistributedRateLimitStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    private readonly Func<DateTimeOffset> _clock;

    public InMemoryDistributedRateLimitStore(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    public bool IsUnavailable { get; set; }

    public Task<DistributedRateLimitLeaseResult> TryAcquireAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        TimeSpan? cooldown,
        CancellationToken cancellationToken = default)
    {
        if (IsUnavailable)
            return Task.FromResult(DistributedRateLimitLeaseResult.Unavailable("Redis unavailable."));

        var now = _clock();

        var entry = _entries.AddOrUpdate(
            key,
            _ => new Entry(1, now.Add(window)),
            (_, current) => current.ExpiresAt <= now
                ? new Entry(1, now.Add(window))
                : current with { Count = current.Count + 1 });

        if (entry.Count <= permitLimit)
            return Task.FromResult(DistributedRateLimitLeaseResult.Acquired(entry.Count, entry.ExpiresAt - now));

        return Task.FromResult(DistributedRateLimitLeaseResult.Rejected(
            entry.Count,
            entry.ExpiresAt - now,
            "Too many requests."));
    }

    public Task ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        _entries.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private sealed record Entry(long Count, DateTimeOffset ExpiresAt);
}