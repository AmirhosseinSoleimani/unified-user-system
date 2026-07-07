using UnifiedUserSystem.src.Application.Abstractions.Auditing;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IOperationRepository Operations {  get; }
    IRoleOperationRepository RoleOperations { get; }
    IAuditLogRepository AuditLogs { get; }
    IRefreshTokenSessionRepository RefreshTokenSessions { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
