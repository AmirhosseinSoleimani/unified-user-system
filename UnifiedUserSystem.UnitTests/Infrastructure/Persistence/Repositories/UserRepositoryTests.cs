using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task ListActiveAsync_Should_ReturnOnlyActiveUsers()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var activeUserB = User.CreateNew(
                "bravo@example.com",
                "bravo",
                "Bravo User",
                "hashed-password-1",
                now,
                actorUserId);

            var inactiveUser = User.CreateNew(
                "charlie@example.com",
                "charlie",
                "Charlie User",
                "hashed-password-2",
                now,
                actorUserId);
            inactiveUser.Deactive(now, actorUserId);

            var activeUserA = User.CreateNew(
                "alpha@example.com",
                "alpha",
                "Alpha User",
                "hashed-password-3",
                now,
                actorUserId);

            dbContext.Users.AddRange(activeUserB, inactiveUser, activeUserA);
            await dbContext.SaveChangesAsync();

            var sut = new UserRepository(dbContext);

            // Act
            var result = await sut.ListActiveAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.IsActive);
            result.Select(x => x.Username).Should().Equal("alpha", "bravo");
        }

        private static AppDbContext CreateDbContext(DateTimeOffset now, Guid actorUserId)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            return new AppDbContext(options, currentUserMock.Object, clockMock.Object);
        }
    }
}
