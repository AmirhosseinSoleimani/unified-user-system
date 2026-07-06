using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Persistence;

public sealed class DomainEventController
{
    public static IReadOnlyCollection<IDomainEvent> CollectDomainEvents(DbContext dbContext, bool clearEvents = true)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var entities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = entities
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        if (clearEvents)
        {
            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }
        }

        return domainEvents;
    }
}
