using Microsoft.EntityFrameworkCore;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

public static class InfrastructureDbContextFactory
{
    public static AppDbContext Create(
        InfrastructureTestClock? clock = null,
        Guid? currentUserId = null,
        bool isAuthenticated = true,
        string? databaseName = null)
    {
        clock ??= new InfrastructureTestClock();

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(currentUserId);
        currentUser.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options, currentUser.Object, clock);
    }

    public static User CreateUser(
        string email = "user@example.com",
        string username = "user",
        string fullname = "Test User",
        string passwordHash = "HASH",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return User.CreateNew(
            email,
            username,
            fullname,
            passwordHash,
            nowUtc ?? new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero),
            actorUserId);
    }

    public static Role CreateRole(
        string key = "admin",
        string name = "Admin",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return Role.Create(
            key,
            name,
            nowUtc ?? new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero),
            actorUserId);
    }

    public static Operation CreateOperation(
        string key = "users.read",
        string title = "Read users",
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return Operation.Create(
            key,
            title,
            nowUtc ?? new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero),
            actorUserId);
    }
}