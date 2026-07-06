namespace Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }
    DateTimeOffset UpdatedAtUtc { get; }
    Guid? CreatedByUserId { get; }
    Guid? UpdatedByUserId { get; }

    void MarkCreated(DateTimeOffset nowUtc, Guid? userId);
    void MarkUpdated(DateTimeOffset nowUtc, Guid? userId);
}