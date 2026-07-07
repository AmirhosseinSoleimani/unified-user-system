using UnifiedUserSystem.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(
            AppDbContext db,
            IUserRepository users,
            IRoleRepository roles,
            IOperationRepository operations,
            IRoleOperationRepository roleOperations,
            IAuditLogRepository auditLogs,
            IRefreshTokenSessionRepository refreshTokenSessions
            )
        {
            _db = db;
            Users = users;
            Roles = roles;
            Operations = operations;
            RoleOperations = roleOperations;
            AuditLogs = auditLogs;
            RefreshTokenSessions = refreshTokenSessions;
        }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IOperationRepository Operations { get; }
        public IRoleOperationRepository RoleOperations { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IRefreshTokenSessionRepository RefreshTokenSessions { get; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
