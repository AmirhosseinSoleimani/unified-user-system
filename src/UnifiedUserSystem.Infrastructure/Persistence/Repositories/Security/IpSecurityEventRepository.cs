using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;

public sealed class IpSecurityEventRepository : IIpSecurityEventRepository
{
    private readonly AppDbContext _db;

    public IpSecurityEventRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Add(IpSecurityEvent securityEvent)
    {
        _db.IpSecurityEvents.Add(securityEvent);
    }

    public async Task<IReadOnlyList<IpSecurityEvent>> SearchAsync(
        IpSecurityEventSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var query = _db.IpSecurityEvents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.NormalizedIpAddress))
            query = query.Where(x => x.NormalizedIpAddress == criteria.NormalizedIpAddress);

        if (criteria.EventType.HasValue)
            query = query.Where(x => x.EventType == criteria.EventType.Value);

        if (criteria.RuleId.HasValue)
            query = query.Where(x => x.RuleId == criteria.RuleId.Value);

        if (criteria.FromUtc.HasValue)
            query = query.Where(x => x.OccurredAt >= criteria.FromUtc.Value);

        if (criteria.ToUtc.HasValue)
            query = query.Where(x => x.OccurredAt <= criteria.ToUtc.Value);

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(500)
            .ToListAsync(ct);
    }
}
