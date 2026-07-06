using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Entities;

public abstract class AuditableAggregateRoot<TKey> : AggregateRoot<TKey>, IAuditableEntity
    where TKey : notnull
{
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public void MarkCreated(DateTimeOffset nowUtc, Guid? userId)
    {
        CreatedAtUtc = nowUtc;
        CreatedByUserId = userId;
        MarkUpdated(nowUtc, userId);
    }

    public void MarkUpdated(DateTimeOffset nowUtc, Guid? userId)
    {
        UpdatedAtUtc = nowUtc;
        UpdatedByUserId = userId;
    }
}