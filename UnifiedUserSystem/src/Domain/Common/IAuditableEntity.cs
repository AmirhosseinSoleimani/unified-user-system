namespace UnifiedUserSystem.src.Domain.Common
{
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        Guid? CreatedByUserId { get; }
        Guid? UpdatedByUserId { get; }
        void SetCreated(DateTimeOffset nowUtc, Guid? userId);
        void Touch(DateTimeOffset nowUtc, Guid? userId);
    }
}
