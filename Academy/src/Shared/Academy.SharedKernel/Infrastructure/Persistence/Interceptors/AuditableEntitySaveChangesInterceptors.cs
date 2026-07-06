using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Security;
using Academy.src.Shared.Academy.SharedKernel.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Persistence.Interceptors;

public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserContext _currentUserContext;

    public AuditableEntitySaveChangesInterceptor(
        IDateTimeProvider dateTimeProvider,
        ICurrentUserContext currentUserContext)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUserContext = currentUserContext;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var nowUtc = _dateTimeProvider.UtcNow;
        var userId = _currentUserContext.UserId;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.MarkDeleted(nowUtc, userId);
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            ApplyAudit(entry, nowUtc, userId);
        }
    }

    private static void ApplyAudit(EntityEntry<IAuditableEntity> entry, DateTimeOffset nowUtc, Guid? userId)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.MarkCreated(nowUtc, userId);
            return;
        }

        if (entry.State == EntityState.Modified)
        {
            entry.Entity.MarkUpdated(nowUtc, userId);
        }
    }
}

