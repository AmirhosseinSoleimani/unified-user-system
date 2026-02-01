using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Repositories
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

        public Task<bool> EmailExistsAsync(string email)
        {
            return _db.Users.AnyAsync(x => x.Email == email);
        }
        public Task<bool> UsernameExistsAsync(string username)
        {
            return _db.Users.AnyAsync(x => x.Username == username);
        }
        public Task<User?> FindEmailOrUsernameAsync(string keyLower)
        {
            return _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Email == keyLower || x.Username.ToLower() == keyLower);
        }

        public Task<User?> FindByIdAsync(Guid id)
        {
            return _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
            
    }
}
