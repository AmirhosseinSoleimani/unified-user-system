using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public sealed class MemoryPermissionCache : IPermissionCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _keysByUser = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _keysByOperation = new();
        private readonly ConcurrentDictionary<string, Guid> _userByCacheKey = new();
        private readonly ConcurrentDictionary<string, string> _operationByCacheKey = new();

        public MemoryPermissionCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<bool?> GetPermissionAsync(
            Guid userId,
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);

            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(operationKey))
                return Task.FromResult<bool?>(null);

            var cacheKey = BuildCacheKey(userId, operationKey);

            if (_memoryCache.TryGetValue<bool>(cacheKey, out var allowed))
                return Task.FromResult<bool?>(allowed);

            return Task.FromResult<bool?>(null);
        }

        public Task SetPermissionAsync(
            Guid userId,
            string operationKey,
            bool allowed,
            TimeSpan ttl,
            CancellationToken cancellationToken = default)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);

            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(operationKey))
                return Task.CompletedTask;

            var cacheKey = BuildCacheKey(userId, operationKey);

            _memoryCache.Set(cacheKey, allowed, ttl);

            _keysByUser
                .GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>())
                .TryAdd(cacheKey, 0);

            _keysByOperation
                .GetOrAdd(operationKey, _ => new ConcurrentDictionary<string, byte>())
                .TryAdd(cacheKey, 0);

            _userByCacheKey[cacheKey] = userId;
            _operationByCacheKey[cacheKey] = operationKey;

            return Task.CompletedTask;
        }

        public Task InvalidateUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                return Task.CompletedTask;

            if (_keysByUser.TryRemove(userId, out var cacheKeys))
            {
                foreach (var cacheKey in cacheKeys.Keys)
                    RemoveCacheKey(cacheKey);
            }

            return Task.CompletedTask;
        }

        public Task InvalidateOperationAsync(
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);

            if (string.IsNullOrWhiteSpace(operationKey))
                return Task.CompletedTask;

            if (_keysByOperation.TryRemove(operationKey, out var cacheKeys))
            {
                foreach (var cacheKey in cacheKeys.Keys)
                    RemoveCacheKey(cacheKey);
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            foreach (var cacheKey in _userByCacheKey.Keys)
                _memoryCache.Remove(cacheKey);

            _keysByUser.Clear();
            _keysByOperation.Clear();
            _userByCacheKey.Clear();
            _operationByCacheKey.Clear();

            return Task.CompletedTask;
        }

        private void RemoveCacheKey(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);

            if (_userByCacheKey.TryRemove(cacheKey, out var userId) &&
                _keysByUser.TryGetValue(userId, out var userCacheKeys))
            {
                userCacheKeys.TryRemove(cacheKey, out _);
            }

            if (_operationByCacheKey.TryRemove(cacheKey, out var operationKey) &&
                _keysByOperation.TryGetValue(operationKey, out var operationCacheKeys))
            {
                operationCacheKeys.TryRemove(cacheKey, out _);
            }
        }

        private static string BuildCacheKey(Guid userId, string operationKey)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);
            return $"permission:{userId:N}:{operationKey}";
        }
    }
}