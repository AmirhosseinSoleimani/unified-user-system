namespace UnifiedUserSystem.src.Domain.Common
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; }
        DateTimeOffset? DeletedAt { get; }
        Guid? DeletedByUserId { get; }

        void SoftDelete(DateTimeOffset nowUtc, Guid? actorUserId);
        void Restore(DateTimeOffset nowUtc, Guid? actorUserId);
    }
}
