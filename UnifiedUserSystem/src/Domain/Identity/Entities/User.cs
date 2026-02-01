using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class User : AuditableEntity<Guid>
    {
        public const int EmailMaxLength = 255;
        public const int UsernameMaxLength = 50;
        public const int FullnameMaxLength = 255;
        public const int PasswordHashMaxLength = 72;
        public string Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Fullname { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public User() { }
        public static User CreateNew(string email, string username, string fullname, string passwordHash, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            email = NormalizeEmail(email);
            username = NormalizeUsername(username);
            fullname = NormalizeFullname(fullname);

            Guard.NotEmpty(email, nameof(email));
            Guard.NotEmpty(username, nameof(username));
            Guard.NotEmpty(fullname, nameof(fullname));
            Guard.NotEmpty(passwordHash, nameof(passwordHash));

            Guard.MaxLen(email, EmailMaxLength, nameof(email));
            Guard.MaxLen(username, UsernameMaxLength, nameof(username));
            Guard.MaxLen(fullname, UsernameMaxLength, nameof(fullname));
            Guard.MaxLen(passwordHash, UsernameMaxLength, nameof(passwordHash));

            var user =  new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                Fullname = fullname,
                PasswordHash = passwordHash,
                IsActive = true,
            };
            user.SetCreated(nowUtc, actorUserId ?? user.Id);
            return user;
        }
        public void ChangeFullName(string newFullName, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newFullName = NormalizeFullname(newFullName);
            Guard.NotEmpty(newFullName, nameof(newFullName));
            Guard.MaxLen(newFullName, FullnameMaxLength, nameof(newFullName));
            if (Fullname == newFullName) return;
            Fullname = newFullName;
            Touch(nowUtc, actorUserId ?? Id);

        }
        public void ChangePasswordHash(string newPasswordHash, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.NotEmpty(newPasswordHash, nameof(newPasswordHash));
            Guard.MaxLen(newPasswordHash, PasswordHashMaxLength, nameof(newPasswordHash));
            if (PasswordHash == newPasswordHash) return;
            PasswordHash = newPasswordHash;
            Touch(nowUtc, actorUserId ?? Id);
        }
        public void Deactive(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!IsActive) return;
            IsActive = false;
            Touch(nowUtc, actorUserId ?? Id);
        }
        public void Active(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (IsActive) return;
            IsActive = true;
            Touch(nowUtc, actorUserId ?? Id);
        }
        public void AssignRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");
            if (UserRoles.Any(x => x.RoleId == roleId)) return;
            UserRoles.Add(UserRole.Create(this.Id, roleId, nowUtc, actorUserId ?? this.Id));
            Touch(nowUtc, actorUserId ?? Id);
        }
        public void RemoveRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (roleId <= 0) throw new ArgumentException("RoleId is invalid.");
            var ur = UserRoles.FirstOrDefault(x => x.RoleId == roleId);
            if (ur is null) return;
            UserRoles.Remove(ur);
            Touch(nowUtc, actorUserId ?? this.Id);
        }
        public static string NormalizeUsername(string username)
        => (username ?? "").Trim();
        public static string NormalizeEmail(string email)
        => (email ?? "").Trim().ToLowerInvariant();
        public static string NormalizeFullname(string fullname)
        => (fullname ?? "").Trim();
    }
}
