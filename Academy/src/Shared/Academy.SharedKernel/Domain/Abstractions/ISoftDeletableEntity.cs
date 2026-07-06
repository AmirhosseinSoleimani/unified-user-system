namespace Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAtUtc { get; }
    Guid? DeletedByUserId { get; }

    void MarkDeleted(DateTimeOffset nowUtc, Guid? actorUserId);
    void MarkRestored(DateTimeOffset nowUtc, Guid? actorUserId);
}
