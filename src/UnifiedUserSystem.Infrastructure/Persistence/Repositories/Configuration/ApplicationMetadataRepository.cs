using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Configuration.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Configuration;

public sealed class ApplicationMetadataRepository : IApplicationMetadataRepository
{
    private readonly AppDbContext _db;

    public ApplicationMetadataRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<ApplicationMetadata?> GetAsync(CancellationToken ct = default)
    {
        return _db.ApplicationMetadata
            .FirstOrDefaultAsync(x => x.Id == ApplicationMetadata.SingletonId, ct);
    }

    public void Add(ApplicationMetadata metadata)
    {
        _db.ApplicationMetadata.Add(metadata);
    }
}
