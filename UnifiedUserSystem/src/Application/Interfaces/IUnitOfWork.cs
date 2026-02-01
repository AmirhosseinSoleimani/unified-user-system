using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IOperationRepository Operations {  get; }
        IRoleOperationRepository RoleOperations { get; }
        Task<int> SaveChangesAsync();
    }
}
