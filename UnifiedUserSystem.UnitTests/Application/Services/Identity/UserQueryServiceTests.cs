using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserQueryServiceTests
    {
        [Fact]
        public async Task ListActiveUsersAsync_ShouldReturnActiveUsersFromRepository()
        {
            var users = new List<User>
            {
                CreateUser("a@example.com", "ali"),
                CreateUser("b@example.com", "bana")
            };

            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();
            repo.Setup(x => x.ListActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
            uow.SetupGet(x => x.Users).Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var result = await sut.ListActiveUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Username == "ali");
            Assert.Contains(result, x => x.Username == "bana");
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnProfile()
        {
            var user = CreateUser("a@example.com", "ali");

            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();
            repo.Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            uow.SetupGet(x => x.Users).Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var result = await sut.GetUserByIdAsync(user.Id);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal("a@example.com", result.Email);
            Assert.Equal("ali", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithMissingUser_ShouldThrowKeyNotFoundException()
        {
            var userId = Guid.NewGuid();
            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();

            repo.Setup(x => x.FindByIdWithRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
            uow.SetupGet(x => x.Users).Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetUserByIdAsync(userId));

            Assert.Equal("User not found.", ex.Message);
        }

        private static User CreateUser(string email, string username)
        {
            return User.CreateNew(email, username, "Full Name", "HASH", DateTimeOffset.UtcNow, Guid.NewGuid());
        }
    }
}
