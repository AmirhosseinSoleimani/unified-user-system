using System.Collections.Concurrent;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public sealed class MemoryTemporarySecurityStateStore : ITemporarySecurityStateStore
    {
        private readonly ConcurrentDictionary<string, Entry> _entries = new();
        private readonly IClock _clock;
        private readonly object _sync = new();

        public MemoryTemporarySecurityStateStore(IClock clock)
        {
            _clock = clock;
        }

        public Task<string?> GetStringAsync(string key, CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(key, out var entry))
                return Task.FromResult<string?>(null);

            if (entry.ExpiresAtUtc <= _clock.Utcnow)
            {
                _entries.TryRemove(key, out _);
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(entry.Value);
        }

        public Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
        {
            _entries[key] = new Entry(value, _clock.Utcnow.Add(ttl));
            return Task.CompletedTask;
        }

        public Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        {
            lock (_sync)
            {
                var now = _clock.Utcnow;

                if (!_entries.TryGetValue(key, out var entry) || entry.ExpiresAtUtc <= now)
                {
                    _entries[key] = new Entry("1", now.Add(ttl));
                    return Task.FromResult(1L);
                }

                var current = long.TryParse(entry.Value, out var parsed) ? parsed : 0;
                var next = current + 1;

                _entries[key] = entry with { Value = next.ToString() };
                return Task.FromResult(next);
            }
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _entries.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private sealed record Entry(string Value, DateTimeOffset ExpiresAtUtc);
    }
}