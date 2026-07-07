using UnifiedUserSystem.src.Domain.Auditing.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Auditing;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}
