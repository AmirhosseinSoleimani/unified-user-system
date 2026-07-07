using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _db;

        public AuditLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(AuditLog auditLog)
        {
            _db.AuditLogs.Add(auditLog);
        }
    }
}
