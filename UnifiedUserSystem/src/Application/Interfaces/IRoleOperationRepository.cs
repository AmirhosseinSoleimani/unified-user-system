using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleOperationRepository
    {
        Task<bool> ExistsAsync(int roleId, Guid operationId);
        Task AddAsync(RoleOperation roleOperation, CancellationToken cancellationToken = default);
        Task<RoleOperation?> FindAsync (int roleId, Guid operationId, CancellationToken cancellationToken = default);
        void Remove(RoleOperation roleOperation);
    }
}
