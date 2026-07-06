using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
}