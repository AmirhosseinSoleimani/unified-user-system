namespace Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAtUtc { get; }
}
