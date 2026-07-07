namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IPermissionCacheInvalidator
{
    Task InvalidateForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateForRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task InvalidateForOperationAsync(string operationKey, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}