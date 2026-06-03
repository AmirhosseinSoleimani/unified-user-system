using UnifiedUserSystem.src.Application.Interfaces.Security;

namespace UnifiedUserSystem.UnitTests.TestHelpers
{
    public sealed class SpyTemporarySecurityStateStore : ITemporarySecurityStateStore
    {
        private readonly Dictionary<string, string> _values = new();

        public List<string> ReadKeys { get; } = new();
        public List<string> SetKeys { get; } = new();
        public List<string> IncrementKeys { get; } = new();
        public List<string> RemovedKeys { get; } = new();
        public List<TimeSpan> SetTtls { get; } = new();
        public List<TimeSpan> IncrementTtls { get; } = new();

        public Task<string?> GetStringAsync(string key, CancellationToken ct = default)
        {
            ReadKeys.Add(key);
            return Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);
        }

        public Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
        {
            SetKeys.Add(key);
            SetTtls.Add(ttl);
            _values[key] = value;
            return Task.CompletedTask;
        }

        public Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        {
            IncrementKeys.Add(key);
            IncrementTtls.Add(ttl);

            var current = _values.TryGetValue(key, out var value) && long.TryParse(value, out var parsed)
                ? parsed
                : 0;

            var next = current + 1;
            _values[key] = next.ToString();

            return Task.FromResult(next);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            RemovedKeys.Add(key);
            _values.Remove(key);
            return Task.CompletedTask;
        }

        public void Seed(string key, string value)
        {
            _values[key] = value;
        }
    }
}