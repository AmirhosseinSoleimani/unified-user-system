using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(User user)
        {
            _db.Users.Add(user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(x => x.Email == email);
        }
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _db.Users.AnyAsync(x => x.Username == username);
        }
        public async Task<User?> FindEmailOrUsernameAsync(string keyLower)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Email == keyLower || x.Username.ToLower() == keyLower);
        }
        public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
        public async Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
            
    }
}
