using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> FindByIdAsync(int id);
        Task<Role?> FindByNameAsync(string nameLower);
        void Add(Role role);
    }
}
