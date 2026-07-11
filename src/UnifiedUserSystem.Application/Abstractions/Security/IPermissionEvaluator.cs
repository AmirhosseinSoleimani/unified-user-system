namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(
        Guid userId,
        string operationKey,
        CancellationToken cancellationToken = default);
}