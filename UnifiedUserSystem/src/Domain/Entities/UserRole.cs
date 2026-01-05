using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class UserRole : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public int RoleId { get; private set; }
        public User User { get; private set; } = default!;
        public Role Role { get; private set; } = default!;

        public UserRole() { }
        public static UserRole Create(Guid userId, int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId
            };
            userRole.SetCreated(nowUtc, actorUserId ?? userId);
            return userRole;
        }
    }
}
