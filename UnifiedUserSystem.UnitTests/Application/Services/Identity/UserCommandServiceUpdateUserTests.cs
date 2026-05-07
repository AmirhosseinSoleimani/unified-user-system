using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity;

public class UserCommandServiceUpdateUserTests
{
    [Fact]
    public async Task UpdateUserAsync_Should_UpdateFullnameOnly_When_OnlyFullnameIsProvided()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var originalUsername = user.Username;
        var originalPasswordHash = user.PasswordHash;
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Fullname = "Updated Fullname"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
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
        var result = await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        result.Fullname.Should().Be("Updated Fullname");
        result.Username.Should().Be(originalUsername);
        user.PasswordHash.Should().Be(originalPasswordHash);

        userRepositoryMock.Verify(x => x.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
        passwordPolicyMock.Verify(x => x.Validate(It.IsAny<string>()), Times.Never);
        passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_UpdateUsernameOnly_When_OnlyUsernameIsProvided()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Username = "newuser"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UsernameExistsAsync("newuser"))
            .ReturnsAsync(false);

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
        var result = await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        result.Username.Should().Be("newuser");
        userRepositoryMock.Verify(x => x.UsernameExistsAsync("newuser"), Times.Once);
        passwordPolicyMock.Verify(x => x.Validate(It.IsAny<string>()), Times.Never);
        passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_UpdatePasswordOnly_ByValidatingAndHashingIt()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Password = "NewPassword123!"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
            .ReturnsAsync(user);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(now);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.Hash("NewPassword123!"))
            .Returns("hashed-new-password");

        var passwordPolicyMock = new Mock<IPasswordPolicy>();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object);

        // Act
        var result = await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        user.PasswordHash.Should().Be("hashed-new-password");
        passwordPolicyMock.Verify(x => x.Validate("NewPassword123!"), Times.Once);
        passwordHasherMock.Verify(x => x.Hash("NewPassword123!"), Times.Once);
        userRepositoryMock.Verify(x => x.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        result.PasswordHashShouldNotExist();
    }

    [Fact]
    public async Task UpdateUserAsync_Should_UpdateAllSupportedFields_When_AllAreProvided()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Fullname = "Updated Name",
            Username = "updateduser",
            Password = "UpdatedPassword123!"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UsernameExistsAsync("updateduser"))
            .ReturnsAsync(false);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(now);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.Hash("UpdatedPassword123!"))
            .Returns("fully-updated-hash");

        var passwordPolicyMock = new Mock<IPasswordPolicy>();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object);

        // Act
        var result = await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        result.Fullname.Should().Be("Updated Name");
        result.Username.Should().Be("updateduser");
        user.PasswordHash.Should().Be("fully-updated-hash");

        userRepositoryMock.Verify(x => x.UsernameExistsAsync("updateduser"), Times.Once);
        passwordPolicyMock.Verify(x => x.Validate("UpdatedPassword123!"), Times.Once);
        passwordHasherMock.Verify(x => x.Hash("UpdatedPassword123!"), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_ThrowDomainException_When_RequestIsNull()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
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
        Func<Task> act = async () => await sut.UpdateUserAsync(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Request is null.");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_ThrowDomainException_When_NoUpdatableFieldIsProvided()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
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

        var req = new UpdateUserRequest();

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(Guid.NewGuid(), req, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("At least one updatable field must be provided.");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_ThrowKeyNotFoundException_When_UserIsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(id, ct))
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

        var req = new UpdateUserRequest { Fullname = "Updated Name" };

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(id, req, ct);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_ThrowInvalidOperationException_When_NewUsernameAlreadyBelongsToAnotherUser()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Username = "takenuser"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UsernameExistsAsync("takenuser"))
            .ReturnsAsync(true);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

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
        Func<Task> act = async () => await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Username already exists.");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotCheckUsernameUniqueness_When_UsernameIsUnchanged()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Username = user.Username
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
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
        var result = await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        result.Username.Should().Be(user.Username);
        userRepositoryMock.Verify(x => x.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_CallSaveChangesAsync_AfterSuccessfulUpdate()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var req = new UpdateUserRequest
        {
            Fullname = "Saved Name"
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
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
        await sut.UpdateUserAsync(user.Id, req, ct);

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

internal static class ProfileResponseAssertionExtensions
{
    public static void PasswordHashShouldNotExist(this UnifiedUserSystem.src.Contracts.DTOs.Profile.ProfileResponse response)
    {
        var propertyNames = response.GetType().GetProperties().Select(x => x.Name).ToArray();
        propertyNames.Should().NotContain("Password");
        propertyNames.Should().NotContain("PasswordHash");
    }
}