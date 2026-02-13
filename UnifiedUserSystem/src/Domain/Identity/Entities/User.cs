using System.Net.Mail;
using System.Text.RegularExpressions;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Ordering.Entities;

namespace UnifiedUserSystem.src.Domain.Identity.Entities
{
    public class User : AuditableEntity<Guid>
    {
        public const int EmailMaxLength = 255;
        public const int UsernameMaxLength = 20;
        public const int UsernameMinLength = 3;
        public const int FullnameMaxLength = 255;
        public const int PasswordHashMaxLength = 72;

        private static readonly HashSet<string> ReserveUsernames = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin", "administrator", "root", "system", "support",
            "null", "undefined", "me", "api", "auth", "profile", "roles", "operations"
        };

        private static readonly Regex UsernameRegex = new(@"^[A-Za-z][A-Za-z0-9_.]{2,19}$", RegexOptions.Compiled);

        public string Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Fullname { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<ProductUser> ProductUsers { get; private set; } = new List<ProductUser>();
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        public User() { }

        public static User CreateNew(string email, string username, string fullname, string passwordHash, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            var newEmail = EnsureValidEmail(email);
            var newUsername = EnsureValidUsername(username);
            var newFullname = EnsureValidFullname(fullname);
            var newPasswordHash = EnsureValidPasswordHash(passwordHash);
        
            var user =  new User
            {
                Id = Guid.NewGuid(),
                Email = newEmail,
                Username = newUsername,
                Fullname = newFullname,
                PasswordHash = newPasswordHash,
                IsActive = true,
            };
            user.SetCreated(nowUtc, actorUserId ?? user.Id);
            return user;
        }
        public void ChangeFullName(string newFullName, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newFullName = EnsureValidFullname(newFullName);

            if (Fullname == newFullName) return;

            Fullname = newFullName;
            Touch(nowUtc, actorUserId ?? Id);

        }
        public void ChangePasswordHash(string newPasswordHash, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newPasswordHash = EnsureValidPasswordHash(newPasswordHash);

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
            UserRoles.Add(UserRole.Create(Id, roleId, nowUtc, actorUserId ?? Id));
            Touch(nowUtc, actorUserId ?? Id);
        }
        public void RemoveRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (roleId <= 0) throw new DomainException("RoleId is invalid.");
            var ur = UserRoles.FirstOrDefault(x => x.RoleId == roleId);
            if (ur is null) return;
            UserRoles.Remove(ur);
            Touch(nowUtc, actorUserId ?? Id);
        }
        public static string NormalizeUsername(string username) => (username ?? "").Trim();
        public static string NormalizeEmail(string email) => (email ?? "").Trim().ToLowerInvariant();
        public static string NormalizeFullname(string fullname) => (fullname ?? "").Trim();
        private static string EnsureValidUsername(string username)
        {
            username = NormalizeUsername(username);

            Guard.NotEmpty(username, nameof(username));
            Guard.AllowedLen(username, UsernameMaxLength, UsernameMinLength, nameof(username));

            if (ReserveUsernames.Contains(username))
                throw new DomainException("username is reserved.");

            if (!UsernameRegex.IsMatch(username))
                throw new DomainException("username format is invalid.");

            if (username.StartsWith('.') || username.EndsWith('.') || username.Contains(".."))
                throw new DomainException("username format is invalid.");

            return username;
        }
        private static string EnsureValidEmail(string email)
        {
            email = NormalizeEmail(email);

            Guard.NotEmpty(email, nameof(email));
            Guard.MaxLen(email, EmailMaxLength, nameof(email));

            try
            {
                var addr = new MailAddress(email);

                if (!string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase))
                    throw new DomainException("email format is invalid.");

                var at = email.LastIndexOf('@');
                if (at < 1 || at == email.Length - 1) 
                    throw new DomainException("email format is invalid.");

                var domain = email[(at + 1)..];
                if (!domain.Contains('.')) 
                    throw new DomainException("email format is invalid.");
            }
            catch
            {
                throw new DomainException("email format is invalid.");
            }
            return email;
        }
        private static string EnsureValidFullname(string fullname) 
        { 
            fullname = NormalizeFullname(fullname);

            Guard.NotEmpty(fullname, nameof(fullname));
            Guard.MaxLen(fullname, FullnameMaxLength, nameof(fullname));

            return fullname;
        }
        private static string EnsureValidPasswordHash(string passwordHash)
        {
            Guard.NotEmpty(passwordHash, nameof(passwordHash));
            Guard.MaxLen(passwordHash, PasswordHashMaxLength, nameof(passwordHash));
            return passwordHash;
        }
    }
}
