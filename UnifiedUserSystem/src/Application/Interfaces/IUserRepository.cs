using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User?> FindEmailOrUsernameAsync(string keyLower);
        Task<User?> FindByIdAsync(Guid id);
        void Add(User user);
    }
}
