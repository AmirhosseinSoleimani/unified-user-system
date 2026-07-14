
using UnifiedUserSystem.src.Domain.Configuration.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IApplicationMetadataRepository
{
    Task<ApplicationMetadata?> GetAsync(CancellationToken ct = default);
    void Add(ApplicationMetadata metadata);
}
