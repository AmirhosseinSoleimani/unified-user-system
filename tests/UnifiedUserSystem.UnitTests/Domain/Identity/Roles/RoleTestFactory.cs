using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Roles;

internal static class RoleTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid ActorUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static Role Create(
        string key = "system-admin",
        string name = "System Administrator",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return Role.Create(
            key: key,
            name: name,
            nowUtc: nowUtc ?? CreatedAt,
            actorUserId: actorUserId);
    }

    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}
