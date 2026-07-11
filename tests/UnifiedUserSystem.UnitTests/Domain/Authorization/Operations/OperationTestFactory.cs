using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Operations;

internal static class OperationTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid ActorUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static Operation Create(
        string key = "users.view",
        string title = "View users",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return Operation.Create(
            key: key,
            title: title,
            nowUtc: nowUtc ?? CreatedAt,
            actoractorUserId: actorUserId);
    }

    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}