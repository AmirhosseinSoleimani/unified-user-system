using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Entities;

public class SoftDeletableEntity<TKey> : AuditableEntity<TKey>, ISoftDeletableEntity
    where TKey : notnull
{
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    public void MarkDeleted(DateTimeOffset nowUtc, Guid? userId)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        DeletedByUserId = userId;
        MarkUpdated(nowUtc, userId);
    }

    public void MarkRestored(DateTimeOffset nowUtc, Guid? userId)
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedByUserId = null;
        MarkUpdated(nowUtc, userId);
    }
}
