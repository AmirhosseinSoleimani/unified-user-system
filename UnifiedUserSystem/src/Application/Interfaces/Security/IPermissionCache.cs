namespace UnifiedUserSystem.src.Application.Interfaces.Security
{
    public interface IPermissionCache
    {
        Task<bool?> GetPermissionAsync(
            Guid userId,
            string operationKey,
            CancellationToken cancellationToken = default);

        Task SetPermissionAsync(
            Guid userId,
            string operationKey,
            bool allowed,
            TimeSpan ttl,
            CancellationToken cancellationToken = default);

        Task InvalidateUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task InvalidateOperationAsync(
            string operationKey,
            CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}