using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Application.Services.Security
{
    internal sealed class NullPermissionCacheInvalidator : IPermissionCacheInvalidator
    {
        public static readonly NullPermissionCacheInvalidator Instance = new();

        private NullPermissionCacheInvalidator()
        {
        }

        public Task InvalidateForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InvalidateForRoleAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InvalidateForOperationAsync(
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}