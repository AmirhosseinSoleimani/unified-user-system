using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOperationService
    {
        Task<IReadOnlyList<Operation>> ListOperationsAsync(CancellationToken ct = default);
        Task<Operation?> GetOperationByIdAsync(Guid operationId, CancellationToken ct = default);
        Task<Operation> CreateOperationAsync(string key, string title, CancellationToken ct = default);
        Task<Operation> UpdateOperationAsync(Guid operationId, string key, string title, CancellationToken ct = default);
        Task DeleteOperationAsync(Guid operationId, CancellationToken ct = default);
        Task RenameOperationTitleAsync(Guid operationId, string newTitle, CancellationToken ct = default);
        Task ChangeOperationKeyAsync(Guid operationId, string newKey, CancellationToken ct = default);
        Task ActivateOperatioAsync(Guid operationId, CancellationToken ct = default);
        Task ActivateOperationAsync(Guid operationId, CancellationToken ct = default);
        Task DeactivateOperationAsync(Guid operationId, CancellationToken ct = default);
    }
}
