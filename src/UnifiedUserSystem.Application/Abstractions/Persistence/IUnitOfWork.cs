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
    ISecuritySettingsRepository SecuritySettings { get; }
    IIpRuleRepository IpRules { get; }
    IIpSecurityEventRepository IpSecurityEvents { get; }
    IMfaChallengeRepository MfaChallenges { get; }
    IApplicationMetadataRepository ApplicationMetadata { get; }
    IErrorMessageRepository ErrorMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
