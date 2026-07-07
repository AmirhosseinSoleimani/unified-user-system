using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Auditing.Entities
{
    public class AuditLog : Entity<Guid>
    {
        public Guid? ActorUserId { get; private set; }
        public Guid? TargetUserId { get; private set; }
        public string EntityName { get; private set; } = default!;
        public string EntityId { get; private set; } = default!;
        public string Action { get; private set; } = default!;
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private AuditLog() { }

        public static AuditLog Create(
            Guid? actorUserId,
            Guid? targetUserId,
            string entityName,
            string entityId,
            string action,
            string? oldValues,
            string? newValues,
            DateTimeOffset createdAt)
        {
            Guard.NotEmpty(entityName, nameof(entityName));
            Guard.NotEmpty(entityId, nameof(entityId));
            Guard.NotEmpty(action, nameof(action));

            return new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                EntityName = entityName.Trim(),
                EntityId = entityId.Trim(),
                Action = action.Trim(),
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = createdAt
            };
        }
    }
}
