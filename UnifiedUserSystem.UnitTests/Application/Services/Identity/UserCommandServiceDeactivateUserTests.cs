using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserCommandServiceDeactivateUserTests
    {
        [Fact]
        public async Task DeactivateUserAsync_Should_DeactivateAnActiveUser()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            var user = CreateUser(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            var passwordPolicyMock = new Mock<IPasswordPolicy>();

            var sut = new UserCommandService(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object,
                passwordHasherMock.Object,
                passwordPolicyMock.Object);

            user.IsActive.Should().BeTrue();

            // Act
            await sut.DeactivateUserAsync(user.Id, ct);

            // Assert
            user.IsActive.Should().BeFalse();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_Should_NotThrow_When_UserIsAlreadyInactive()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            var user = CreateUser(now, actorUserId);

            user.Deactive(now, actorUserId);
            user.IsActive.Should().BeFalse();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            var passwordPolicyMock = new Mock<IPasswordPolicy>();

            var sut = new UserCommandService(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object,
                passwordHasherMock.Object,
                passwordPolicyMock.Object);

            // Act
            var act = async () => await sut.DeactivateUserAsync(user.Id, ct);

            // Assert
            await act.Should().NotThrowAsync();
            user.IsActive.Should().BeFalse();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_Should_ThrowKeyNotFoundException_When_UserIsMissing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(userId, ct))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            var currentUserMock = new Mock<ICurrentUser>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var passwordPolicyMock = new Mock<IPasswordPolicy>();

            var sut = new UserCommandService(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object,
                passwordHasherMock.Object,
                passwordPolicyMock.Object);

            // Act
            var act = async () => await sut.DeactivateUserAsync(userId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task DeactivateUserAsync_Should_CallSaveChangesAsync_AfterSuccessfulDeactivation()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            var user = CreateUser(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            var passwordPolicyMock = new Mock<IPasswordPolicy>();

            var sut = new UserCommandService(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object,
                passwordHasherMock.Object,
                passwordPolicyMock.Object);

            // Act
            await sut.DeactivateUserAsync(user.Id, ct);

            // Assert
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        private static User CreateUser(DateTimeOffset now, Guid actorUserId)
        {
            return User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice Doe",
                "initial-password-hash",
                now,
                actorUserId);
        }
    }
}
