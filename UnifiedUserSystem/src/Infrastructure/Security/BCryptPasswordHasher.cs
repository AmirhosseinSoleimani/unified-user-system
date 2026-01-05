using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 8);


        public bool Verify(string password, string passwordHasd)
            => BCrypt.Net.BCrypt.Verify(password, passwordHasd);
    }
}
