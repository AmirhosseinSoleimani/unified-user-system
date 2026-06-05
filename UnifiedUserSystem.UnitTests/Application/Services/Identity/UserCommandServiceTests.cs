using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserCommandServiceTests
    {
        private static readonly DateTimeOffset Now = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

        [Fact]
        public async Task UpdateUserAsync_WithValidFullname_ShouldUpdateUser_Save_AndWriteAuditLog()
        {
            var f = new Fixture();
            var user = CreateUser();
            var req = new UpdateUserRequest { Fullname = " New Name " };

            f.Users.Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var result = await f.Sut.UpdateUserAsync(user.Id, req);

            Assert.Equal("New Name", result.Fullname);
            Assert.Equal(Now, user.UpdatedAt);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            f.Audit.Verify(x => x.WriteAsync(It.Is<WriteAuditLogRequest>(r =>
                r.Action == "UserUpdated" &&
                r.TargetUserId == user.Id &&
                r.NewValues!.ContainsKey("Fullname")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateUsername_ShouldThrow_AndNotSave()
        {
            var f = new Fixture();
            var user = CreateUser();
            var req = new UpdateUserRequest { Username = "other123" };

            f.Users.Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            f.Users.Setup(x => x.UsernameExistsAsync("other123")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.UpdateUserAsync(user.Id, req));

            Assert.Equal("Username already exists.", ex.Message);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WithPassword_ShouldValidatePolicy_HashPassword_Save_AndNotStorePlainPassword()
        {
            var f = new Fixture();
            var user = CreateUser();
            var req = new UpdateUserRequest { Password = "Password1!" };

            f.Users.Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            f.Hasher.Setup(x => x.Hash("Password1!")).Returns("NEW_HASH");

            await f.Sut.UpdateUserAsync(user.Id, req);

            Assert.Equal("NEW_HASH", user.PasswordHash);
            Assert.NotEqual("Password1!", user.PasswordHash);
            f.PasswordPolicy.Verify(x => x.Validate("Password1!"), Times.Once);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithMissingUser_ShouldThrowKeyNotFoundException()
        {
            var f = new Fixture();
            var userId = Guid.NewGuid();

            f.Users.Setup(x => x.FindByIdWithRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                f.Sut.UpdateUserAsync(userId, new UpdateUserRequest { Fullname = "Name" }));

            Assert.Equal("User not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_WithEmptyRequest_ShouldThrowDomainException()
        {
            var f = new Fixture();

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                f.Sut.UpdateUserAsync(Guid.NewGuid(), new UpdateUserRequest()));

            Assert.Equal("At least one updatable field must be provided.", ex.Message);
        }

        [Fact]
        public async Task DeactivateUserAsync_WithExistingUser_ShouldDeactivate_Save_InvalidateCache_AndWriteAuditLog()
        {
            var f = new Fixture();
            var user = CreateUser();

            f.Users.Setup(x => x.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await f.Sut.DeactivateUserAsync(user.Id);

            Assert.False(user.IsActive);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            f.PermissionInvalidator.Verify(x => x.InvalidateForUserAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
            f.Audit.Verify(x => x.WriteAsync(It.Is<WriteAuditLogRequest>(r =>
                r.Action == "UserDeactivated" && r.TargetUserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_WithMissingUser_ShouldThrowKeyNotFoundException()
        {
            var f = new Fixture();
            var userId = Guid.NewGuid();

            f.Users.Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => f.Sut.DeactivateUserAsync(userId));

            Assert.Equal("User not found.", ex.Message);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static User CreateUser()
        {
            return User.CreateNew("user@example.com", "user123", "Full Name", "HASH", Now.AddMinutes(-5), Guid.NewGuid());
        }

        private sealed class Fixture
        {
            public Mock<IUnitOfWork> Uow { get; } = new();
            public Mock<IUserRepository> Users { get; } = new();
            public Mock<IClock> Clock { get; } = new();
            public Mock<ICurrentUser> CurrentUser { get; } = new();
            public Mock<IPasswordHasher> Hasher { get; } = new();
            public Mock<IPasswordPolicy> PasswordPolicy { get; } = new();
            public Mock<IAuditLogWriter> Audit { get; } = new();
            public Mock<IPermissionCacheInvalidator> PermissionInvalidator { get; } = new();

            public UserCommandService Sut { get; }

            public Fixture()
            {
                Uow.SetupGet(x => x.Users).Returns(Users.Object);
                Clock.SetupGet(x => x.Utcnow).Returns(Now);
                CurrentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
                CurrentUser.SetupGet(x => x.IsAuthenticated).Returns(true);

                Sut = new UserCommandService(
                    Uow.Object,
                    Clock.Object,
                    CurrentUser.Object,
                    Hasher.Object,
                    PasswordPolicy.Object,
                    Audit.Object,
                    PermissionInvalidator.Object);
            }
        }
    }
}