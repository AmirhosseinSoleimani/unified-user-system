using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserQueryServiceTests
    {
        [Fact]
        public async Task ListActiveUsersAsync_ShouldReturnActiveUsersFromRepository()
        {
            var users = new List<User>
            {
                CreateUser("a@example.com", "ali", "Ali", "Ahmadi", "+989111111111"),
                CreateUser("b@example.com", "bana", "Bana", "Karimi", "+989222222222")
            };

            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();

            repo.Setup(x => x.ListActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            uow.SetupGet(x => x.Users)
                .Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var result = await sut.ListActiveUsersAsync();

            Assert.Equal(2, result.Count);

            Assert.Contains(result, x =>
                x.Email == "a@example.com" &&
                x.Username == "ali" &&
                x.FirstName == "Ali" &&
                x.LastName == "Ahmadi" &&
                x.PhoneNumber == "+989111111111" &&
                x.Fullname == "Ali Ahmadi");

            Assert.Contains(result, x =>
                x.Email == "b@example.com" &&
                x.Username == "bana" &&
                x.FirstName == "Bana" &&
                x.LastName == "Karimi" &&
                x.PhoneNumber == "+989222222222" &&
                x.Fullname == "Bana Karimi");
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnProfile()
        {
            var user = CreateUser(
                email: "a@example.com",
                username: "ali",
                firstName: "Ali",
                lastName: "Ahmadi",
                phoneNumber: "+989123456789");

            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();

            repo.Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            uow.SetupGet(x => x.Users)
                .Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var result = await sut.GetUserByIdAsync(user.Id);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal("a@example.com", result.Email);
            Assert.Equal("ali", result.Username);
            Assert.Equal("Ali", result.FirstName);
            Assert.Equal("Ahmadi", result.LastName);
            Assert.Equal("+989123456789", result.PhoneNumber);
            Assert.Equal("Ali Ahmadi", result.Fullname);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithMissingUser_ShouldThrowKeyNotFoundException()
        {
            var userId = Guid.NewGuid();
            var repo = new Mock<IUserRepository>();
            var uow = new Mock<IUnitOfWork>();

            repo.Setup(x => x.FindByIdWithRolesAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            uow.SetupGet(x => x.Users)
                .Returns(repo.Object);

            var sut = new UserQueryService(uow.Object);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetUserByIdAsync(userId));

            Assert.Equal("User not found.", ex.Message);
        }

        private static User CreateUser(
            string email,
            string username,
            string firstName,
            string lastName,
            string phoneNumber)
        {
            return User.CreateNew(
                email: email,
                username: username,
                firstName: firstName,
                lastName: lastName,
                phoneNumber: phoneNumber,
                passwordHash: "HASH",
                nowUtc: DateTimeOffset.UtcNow,
                actorUserId: Guid.NewGuid());
        }
    }
}