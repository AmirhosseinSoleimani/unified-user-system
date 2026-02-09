using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IOperationRepository
    {
        Task<Operation?> FindByIdAsync(Guid id);
        Task<Operation?> FindByKeyAsync(string keyLower);
        void Add(Operation operation);
    }
}
