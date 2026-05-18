using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOperationRepository
    {
        Task<IReadOnlyList<Operation>> ListAsync(CancellationToken ct = default);
        Task<Operation?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<Operation?> FindByKeyAsync(string keyLower, CancellationToken ct = default);
        Task<bool> HasAssignedRolesAsync(Guid operationId, CancellationToken ct = default);
        void Add(Operation operation);
    }
}
