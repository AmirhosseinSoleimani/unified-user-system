using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserQueryServiceTests
    {
        public async Task ListActiveUsersAsync_Should_ReturnMappedActiveUsers_When_RepositoryReturnsUsers()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;
            var actorUserId = Guid.NewGuid();
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);

            var users = new List<User>
        {
            User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice Doe",
                "hashed-password-1",
                now,
                actorUserId),
            User.CreateNew(
                "bob@example.com",
                "bob",
                "Bob Doe",
                "hashed-password-2",
                now,
                actorUserId)
        };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.ListActiveAsync(ct))
                .ReturnsAsync(users);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.Users)
                .Returns(userRepositoryMock.Object);

            var sut = new UserQueryService(unitOfWorkMock.Object);

            // Act
            var result = await sut.ListActiveUsersAsync(ct);

            // Assert
            userRepositoryMock.Verify(x => x.ListActiveAsync(ct), Times.Once);

            result.Should().HaveCount(2);
            _ = result.Should().BeEquivalentTo(
                [
                new
                {
                    users[0].Id,
                    users[0].Email,
                    users[0].Username,
                    users[0].Fullname,
                    users[0].IsActive
                },
                new
                {
                    users[1].Id,
                    users[1].Email,
                    users[1].Username,
                    users[1].Fullname,
                    users[1].IsActive
                }
                ]);
        }
    }
}
