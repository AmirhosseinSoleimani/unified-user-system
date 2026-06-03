using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.src.Application.Services.Security
{
    public sealed class PermissionCacheInvalidator : IPermissionCacheInvalidator
    {
        private readonly IPermissionCache _cache;
        private readonly IPermissionReadRepository _repository;

        public PermissionCacheInvalidator(
            IPermissionCache cache,
            IPermissionReadRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public Task InvalidateForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                return Task.CompletedTask;

            return _cache.InvalidateUserAsync(userId, cancellationToken);
        }

        public async Task InvalidateForRoleAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            if (roleId <= 0)
                return;

            var userIds = await _repository.ListUserIdsInRoleAsync(
                roleId,
                cancellationToken);

            foreach (var userId in userIds)
            {
                await _cache.InvalidateUserAsync(userId, cancellationToken);
            }
        }

        public Task InvalidateForOperationAsync(
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);

            if (string.IsNullOrWhiteSpace(operationKey))
                return Task.CompletedTask;

            return _cache.InvalidateOperationAsync(operationKey, cancellationToken);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            return _cache.ClearAsync(cancellationToken);
        }
    }
}