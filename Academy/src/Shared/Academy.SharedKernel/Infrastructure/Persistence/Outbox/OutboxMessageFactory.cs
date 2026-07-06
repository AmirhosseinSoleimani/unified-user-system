using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Serialization;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageFactory
{
    private readonly IEventSerializer _serializer;

    public OutboxMessageFactory(IEventSerializer serializer)
    {
        _serializer = serializer;
    }

    public OutboxMessage Create(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var content = _serializer.Serialize(domainEvent, eventType);

        return OutboxMessage.Create(
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
            content);
    }
}
