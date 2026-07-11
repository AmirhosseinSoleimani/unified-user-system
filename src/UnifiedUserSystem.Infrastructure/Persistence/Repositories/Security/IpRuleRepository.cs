using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;

public sealed class IpRuleRepository : IIpRuleRepository
{
    private readonly AppDbContext _db;

    public IpRuleRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<IpRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.IpRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<IpRule>> ListAsync(CancellationToken ct = default)
    {
        return await _db.IpRules
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<IpRule>> ListActiveAsync(CancellationToken ct = default)
    {
        return await _db.IpRules
            .Where(x => x.IsActive)
            .ToListAsync(ct);
    }

    public void Add(IpRule rule)
    {
        _db.IpRules.Add(rule);
    }
}
