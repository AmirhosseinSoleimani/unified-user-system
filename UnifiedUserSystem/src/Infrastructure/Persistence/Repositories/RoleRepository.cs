using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _db;
        public RoleRepository(AppDbContext db) 
        {
            _db = db;
        }
        public void Add(Role role)
        {
            _db.Roles.Add(role);
        }

        public Task<Role?> FindByIdAsync(int id)
        {
            return _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Role?> FindByNameAsync(string nameLower)
        {
            return _db.Roles.FirstOrDefaultAsync(x => x.Name == nameLower);
        }
    }
}
