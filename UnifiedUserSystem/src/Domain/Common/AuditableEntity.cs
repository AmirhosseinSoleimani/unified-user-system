namespace UnifiedUserSystem.src.Domain.Common
{
    public abstract class AuditableEntity<TKey> : Entity<TKey> , IAuditableEntity where TKey : struct
    {
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public Guid? CreatedByUserId { get; private set; }
        public Guid? UpdatedByUserId { get; private set; }
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
    }
}
