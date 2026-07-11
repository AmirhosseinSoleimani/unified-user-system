
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.UserRoles;

internal static class UserRoleTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static readonly Guid ActorUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal const int RoleId = 10;

    internal static UserRole Create(
        Guid? userId = null,
        int roleId = RoleId,
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return UserRole.Create(
            userId: userId ?? UserId,
            roleId: roleId,
            nowUtc: nowUtc ?? CreatedAt,
            actorUserId: actorUserId);
    }
}
