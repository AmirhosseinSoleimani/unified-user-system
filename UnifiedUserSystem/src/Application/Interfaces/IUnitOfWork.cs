using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Identity;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IOperationRepository Operations {  get; }
        IRoleOperationRepository RoleOperations { get; }
        IAuditLogRepository AuditLogs { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
