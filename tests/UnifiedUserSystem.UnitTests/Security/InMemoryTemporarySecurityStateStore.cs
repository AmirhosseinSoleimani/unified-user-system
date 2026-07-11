
using System.Collections.Concurrent;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.UnitTests.Security;

internal sealed class InMemoryTemporarySecurityStateStore : ITemporarySecurityStateStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    private readonly Func<DateTimeOffset> _clock;

    public InMemoryTemporarySecurityStateStore(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    public bool IsUnavailable { get; set; }

    public Task<string?> GetStringAsync(string key, CancellationToken ct = default)
    {
        if (IsUnavailable)
            throw new SecurityStateUnavailableException("Redis unavailable.");

        if (!_entries.TryGetValue(key, out var entry) || entry.ExpiresAt <= _clock())
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>(entry.Value);
    }

    public Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
    {
        if (IsUnavailable)
            throw new SecurityStateUnavailableException("Redis unavailable.");

        _entries[key] = new Entry(value, _clock().Add(ttl));
        return Task.CompletedTask;
    }

    public Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        if (IsUnavailable)
            throw new SecurityStateUnavailableException("Redis unavailable.");

        var now = _clock();

        var entry = _entries.AddOrUpdate(
            key,
            _ => new Entry("1", now.Add(ttl)),
            (_, current) =>
            {
                if (current.ExpiresAt <= now)
                    return new Entry("1", now.Add(ttl));

                var next = long.Parse(current.Value) + 1;
                return current with { Value = next.ToString() };
            });

        return Task.FromResult(long.Parse(entry.Value));
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _entries.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private sealed record Entry(string Value, DateTimeOffset ExpiresAt);
}
