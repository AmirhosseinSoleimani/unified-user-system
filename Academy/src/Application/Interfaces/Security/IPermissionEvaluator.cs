namespace UnifiedUserSystem.src.Application.Interfaces.Security
{
    public interface IPermissionEvaluator
    {
        Task<bool> HasPermissionAsync(
            Guid userId,
            string operationKey,
            CancellationToken cancellationToken = default);
    }
}