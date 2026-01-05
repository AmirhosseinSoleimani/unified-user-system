using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class User : AuditableEntity<Guid>
    {
        public string Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Fullname { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public User() { }
        public static User CreateNew(string email, string username, string fullname, string passwordHash, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            email = NormalizeEmail(email);
            username = NormalizeUsername(username);
            fullname = NormalizeFullname(fullname);

            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(fullname)) throw new ArgumentException("FullName is required.");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required.");

            var user =  new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                Fullname = fullname,
                PasswordHash = passwordHash,
            };
            user.SetCreated(nowUtc, actorUserId ?? user.Id);
            return user;
        }
        public void AssignRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (UserRoles.Any(x => x.RoleId == roleId)) return;
            UserRoles.Add(UserRole.Create(this.Id, roleId, nowUtc, actorUserId ?? this.Id));

        }
        public void ChangeFullName(string newFullName, DateTimeOffset nowUtc, Guid? actorUserId) 
        {
            newFullName = NormalizeFullname(newFullName);
            if (string.IsNullOrWhiteSpace(newFullName))
                throw new ArgumentException("FullName is required.");
            Fullname = newFullName;
            Touch(nowUtc, actorUserId);
        }
        public static string NormalizeUsername(string username)
        => (username ?? "").Trim();
        public static string NormalizeEmail(string email)
        => (email ?? "").Trim().ToLowerInvariant();
        public static string NormalizeFullname(string fullname)
        => (fullname ?? "").Trim();
    }
}
