namespace UnifiedUserSystem.src.Domain.Common
{
    public abstract class AuditableEntity<TKey> : Entity<TKey> , ISoftDeletable, IAuditableEntity where TKey : struct
    {
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public Guid? CreatedByUserId { get; private set; }
        public Guid? UpdatedByUserId { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }
        public Guid? DeletedByUserId { get; private set; }

        public void SetCreated(DateTimeOffset nowUtc, Guid? userId)
        {
            CreatedAt = nowUtc;
            UpdatedAt = nowUtc;
            CreatedByUserId = userId;
            UpdatedByUserId = userId;
        }
        public void Touch(DateTimeOffset nowUtc, Guid? userId) 
        {
            UpdatedAt = nowUtc;
            UpdatedByUserId = userId;
        }

        public void SoftDelete(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (IsDeleted) return;

            IsDeleted = true;
            DeletedAt = nowUtc;
            DeletedByUserId = actorUserId;

            Touch(nowUtc, actorUserId);
        }

        public void Restore(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!IsDeleted) return;

            IsDeleted = false;
            DeletedAt = null;
            DeletedByUserId = actorUserId;

            Touch(nowUtc, actorUserId);
        }
    }
}
