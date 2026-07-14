using UnifiedUserSystem.src.Domain.Localization.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IErrorMessageRepository
{
    Task<IReadOnlyList<ErrorMessage>> ListAsync(bool activeOnly = false, CancellationToken ct = default);
    Task<ErrorMessage?> FindByKeyAsync(string key, CancellationToken ct = default);
    void Add(ErrorMessage message);
}
