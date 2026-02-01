using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOperationService
    {
        Task<Operation> CreateOperationAsync(string key, string title, CancellationToken ct = default);
        Task RenameOperationTitleAsync(Guid operationId, string newTitle, CancellationToken ct = default);
        Task ChangeOperationKeyAsync(Guid operationId, string newKey, CancellationToken ct = default);
        Task ActivateOperatioAsync(Guid operationId, CancellationToken ct = default);
        Task DeactivateOperationAsync(Guid operationId, CancellationToken ct = default);
    }
}
