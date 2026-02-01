namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IPermissionService
    {
        Task GrantOperationToRoleAsync(int roleId, Guid operationId, CancellationToken ct = default);
        Task RevokeOperationFromRoleAsync(int roleId, Guid operationId, CancellationToken ct = default);
    }
}
