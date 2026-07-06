using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.src.Application.Services.Security
{
    public sealed class PermissionEvaluator : IPermissionEvaluator
    {
        private readonly IPermissionCache _cache;
        private readonly IPermissionReadRepository _repository;
        private readonly PermissionEvaluationOptions _options;

        public PermissionEvaluator(
            IPermissionCache cache,
            IPermissionReadRepository repository,
            IOptions<PermissionEvaluationOptions> options)
        {
            _cache = cache;
            _repository = repository;
            _options = options.Value;
        }

        public async Task<bool> HasPermissionAsync(
            Guid userId,
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                return false;

            operationKey = NormalizeOperationKey(operationKey);

            if (string.IsNullOrWhiteSpace(operationKey))
                return false;

            var cached = await _cache.GetPermissionAsync(userId, operationKey, cancellationToken);
            if (cached.HasValue)
                return cached.Value;

            var allowed = await _repository.UserHasOperationAsync(userId, operationKey, cancellationToken);

            var ttl = TimeSpan.FromSeconds(_options.CacheTtlSeconds <= 0 ? 60 : _options.CacheTtlSeconds);
            await _cache.SetPermissionAsync(userId, operationKey, allowed, ttl, cancellationToken);

            return allowed;
        }

        private static string NormalizeOperationKey(string operationKey)
        {
            operationKey = (operationKey ?? string.Empty).Trim();

            if (operationKey.StartsWith(OperationPolicyNames.Prefix, StringComparison.OrdinalIgnoreCase))
                operationKey = operationKey[OperationPolicyNames.Prefix.Length..];

            return operationKey.Trim().ToLowerInvariant();
        }
    }
}