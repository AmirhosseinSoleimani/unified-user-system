
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class Role : AuditableEntity<int>
    {
        public const int NameMaxLength = 50;
        public string Name { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<RoleOperation> RoleOperations { get; private set; } = new List<RoleOperation>();
        public Role() { }
        public static Role Create(string name, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            name = NormalizeName(name);
            Guard.NotEmpty(name, nameof(name));
            Guard.MaxLen(name, NameMaxLength, nameof(name));
            var role = new Role
            {
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
        public void Deactive(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!IsActive) return;
            IsActive = false;
            Touch(nowUtc, actorUserId);
        }
        public void Active(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (IsActive) return;
            IsActive = true;
            Touch(nowUtc, actorUserId);
        }
        public static string NormalizeName(string name)
            => (name ?? "").Trim().ToLowerInvariant();
    }
}
