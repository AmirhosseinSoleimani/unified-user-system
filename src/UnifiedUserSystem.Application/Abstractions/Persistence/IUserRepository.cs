using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<bool> UsernameExistsAsync(string username);
    Task<User?> FindEmailOrUsernameAsync(string keyLower);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken ct = default);
    void Add(User user);
}
