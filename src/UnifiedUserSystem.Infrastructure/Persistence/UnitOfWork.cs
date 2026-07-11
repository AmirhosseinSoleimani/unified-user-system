using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Auditing;
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
            IRefreshTokenSessionRepository refreshTokenSessions,
            ISecuritySettingsRepository securitySettings,
            IIpRuleRepository ipRules,
            IIpSecurityEventRepository ipSecurityEvents,
            IMfaChallengeRepository mfaChallenges
            )
        {
            _db = db;
            Users = users;
            Roles = roles;
            Operations = operations;
            RoleOperations = roleOperations;
            AuditLogs = auditLogs;
            RefreshTokenSessions = refreshTokenSessions;
            SecuritySettings = securitySettings;
            IpRules = ipRules;
            IpSecurityEvents = ipSecurityEvents;
            MfaChallenges = mfaChallenges;
        }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IOperationRepository Operations { get; }
        public IRoleOperationRepository RoleOperations { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IRefreshTokenSessionRepository RefreshTokenSessions { get; }
        public ISecuritySettingsRepository SecuritySettings { get; }
        public IIpRuleRepository IpRules { get; }
        public IIpSecurityEventRepository IpSecurityEvents { get; }
        public IMfaChallengeRepository MfaChallenges { get; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
