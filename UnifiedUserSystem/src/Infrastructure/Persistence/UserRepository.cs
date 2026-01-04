using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence
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

        public Task<User?> FindEmailOrUsernameAsync(string emailOrUsernameLower)
        {
            return _db.Users.FirstOrDefaultAsync(x => x.Email == emailOrUsernameLower || x.Username.ToLower() == emailOrUsernameLower);
        }
        public Task<bool> UsernameExistsAsync(string username)
        {
            return _db.Users.AnyAsync(x => x.Username == username);
        }
    }
}
