using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Fullname { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public DateTimeOffset? CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public string Role { get; private set; } = "user";
        public User() { }
        public static User CreateNew(string email, string username, string fullname, string passwordHash, DateTimeOffset nowUtc)
        {
            email = NormalizeEmail(email);
            username = NormalizeUsername(username);
            fullname = NormalizeFullname(fullname);

            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(fullname)) throw new ArgumentException("FullName is required.");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required.");

            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                Fullname = fullname,
                PasswordHash = passwordHash,
                Role = "user",
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            };
        }
        public void ChangeFullName(string newFullName, DateTimeOffset nowUtc) 
        {
            newFullName = NormalizeFullname(newFullName);
            if (string.IsNullOrWhiteSpace(newFullName))
                throw new ArgumentException("FullName is required.");
            Fullname = newFullName;
            Touch(nowUtc);
        }
        public void Touch(DateTimeOffset nowUtc)
        {
            UpdatedAt = nowUtc;
        }
        public static string NormalizeUsername(string username)
        => (username ?? "").Trim();
        public static string NormalizeEmail(string email)
        => (email ?? "").Trim().ToLowerInvariant();
        public static string NormalizeFullname(string fullname)
        => (fullname ?? "").Trim();
    }
}
