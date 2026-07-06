using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleOperationRepository
    {
        Task<IReadOnlyList<RoleOperation>> ListByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int roleId, Guid operationId, CancellationToken cancellationToken = default);
        Task AddAsync(RoleOperation roleOperation, CancellationToken cancellationToken = default);
        Task<RoleOperation?> FindAsync(int roleId, Guid operationId, CancellationToken cancellationToken = default);
        void Remove(RoleOperation roleOperation);
    }
}
