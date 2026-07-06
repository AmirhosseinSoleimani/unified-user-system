using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
