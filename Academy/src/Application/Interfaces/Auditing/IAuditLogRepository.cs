using UnifiedUserSystem.src.Domain.Auditing.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces.Auditing
{
    public interface IAuditLogRepository
    {
        void Add(AuditLog auditLog);
    }
}
