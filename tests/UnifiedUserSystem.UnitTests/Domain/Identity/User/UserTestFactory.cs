using UserEntity =
    UnifiedUserSystem.src.Domain.Identity.Entities.User;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

internal static class UserTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid ActorUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static UserEntity Create(
        string email = "user@example.com",
        string username = "test.user",
        string firstName = "Amirhossein",
        string lastName = "Soleimani",
        string phoneNumber = "09123456789",
        string passwordHash = "valid-password-hash",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return UserEntity.CreateNew(
            email: email,
            username: username,
            firstName: firstName,
            lastName: lastName,
            phoneNumber: phoneNumber,
            passwordHash: passwordHash,
            nowUtc: nowUtc ?? CreatedAt,
            actorUserId: actorUserId);
    }

    internal static string CreateString(
        int length,
        char character = 'a')
    {
        return new string(character, length);
    }
}