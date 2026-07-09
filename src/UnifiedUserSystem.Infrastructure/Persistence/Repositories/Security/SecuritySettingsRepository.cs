using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Security;

public sealed class SecuritySettingsRepository : ISecuritySettingsRepository
{
    private readonly AppDbContext _db;

    public SecuritySettingsRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<SecuritySettings?> GetAsync(CancellationToken ct = default)
    {
        return _db.SecuritySettings
            .FirstOrDefaultAsync(x => x.Id == SecuritySettings.SingletonId, ct);
    }

    public void Add(SecuritySettings settings)
    {
        _db.SecuritySettings.Add(settings);
    }
}
