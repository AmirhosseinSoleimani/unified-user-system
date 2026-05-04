using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Identity.Entities
{
    public class Role : AuditableEntity<int>
    {
        public const int NameMaxLength = 50;
        public const int KeyMaxLength = 50;
        public string Name { get; private set; } = default!;
        public string Key { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<RoleOperation> RoleOperations { get; private set; } = new List<RoleOperation>();

        private Role() { }
        public static Role Create(string key, string name, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            key = NormalizeKey(key);
            name = NormalizeName(name);

            Guard.NotEmpty(key, nameof(key));
            Guard.MaxLen(key, KeyMaxLength, nameof(key));

            Guard.NotEmpty(name, nameof(name));
            Guard.MaxLen(name, NameMaxLength, nameof(name));

            var role = new Role
            {
                Key = key,
                Name = name,
                IsActive = true
            };

            role.SetCreated(nowUtc, actorUserId);
            return role;
        }

        public void Rename(string newName, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newName = NormalizeName(newName);

            Guard.NotEmpty(newName, nameof(newName));
            Guard.MaxLen(newName, NameMaxLength, nameof(newName));

            if (Name == newName) return;

            Name = newName;
            Touch(nowUtc, actorUserId);
        }
        public void ChangeKey(string newKey, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newKey = NormalizeKey(newKey);

            Guard.NotEmpty(newKey, nameof(newKey));
            Guard.MaxLen(newKey, KeyMaxLength, nameof(newKey));

            if (Key == newKey) return;

            Key = newKey;
            Touch(nowUtc, actorUserId);
        }
        public void Deactivate(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!IsActive) return;
            IsActive = false;
            Touch(nowUtc, actorUserId);
        }
        public void Activate(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (IsActive) return;
            IsActive = true;
            Touch(nowUtc, actorUserId);
        }

        public void Delete(DateTimeOffset nowUtc, Guid? actorUserId)
            => SoftDelete(nowUtc, actorUserId);
        public void UnDelete(DateTimeOffset nowUtc, Guid? actorUserId)
            => Restore(nowUtc, actorUserId);

        public static string NormalizeName(string name)
            => (name ?? "").Trim();
        public static string NormalizeKey(string key) 
        {
            var newKey = (key ?? "").Trim().ToLowerInvariant();
            newKey = string.Join("-", newKey.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return newKey;

        }
    }
}
