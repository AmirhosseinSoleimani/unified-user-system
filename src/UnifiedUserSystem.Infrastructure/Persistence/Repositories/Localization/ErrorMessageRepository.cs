
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Localization.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Localization;

public sealed class ErrorMessageRepository : IErrorMessageRepository
{
    private readonly AppDbContext _db;

    public ErrorMessageRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ErrorMessage>> ListAsync(bool activeOnly = false, CancellationToken ct = default)
    {
        var query = _db.ErrorMessages.AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.Key)
            .ToListAsync(ct);
    }

    public Task<ErrorMessage?> FindByKeyAsync(string key, CancellationToken ct = default)
    {
        key = ErrorMessage.NormalizeKey(key);

        return _db.ErrorMessages
            .FirstOrDefaultAsync(x => x.Key == key, ct);
    }

    public void Add(ErrorMessage message)
    {
        _db.ErrorMessages.Add(message);
    }
}
