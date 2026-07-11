namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IPermissionReadRepository
{
    Task<bool> UserHasOperationAsync(
        Guid userId,
        string operationKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ListUserIdsInRoleAsync(
        int roleId,
        CancellationToken cancellationToken = default);
}