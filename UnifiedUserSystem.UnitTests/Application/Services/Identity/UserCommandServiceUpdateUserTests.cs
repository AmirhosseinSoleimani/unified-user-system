using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
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

        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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

        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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

        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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

        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, req, ct);

        // Assert
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_WriteAuditLogOnce_When_UpdateSucceeds()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Fullname = "Updated Fullname" }, ct);

        // Assert
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_WriteAuditLogWithFullnameOldAndNewValues_When_FullnameChanges()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;
        WriteAuditLogRequest? capturedRequest = null;

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
        var auditLogWriterMock = CreateAuditLogWriterMock(request => capturedRequest = request);

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Fullname = "Updated Fullname" }, ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ActorUserId.Should().Be(actorUserId);
        capturedRequest.TargetUserId.Should().Be(user.Id);
        capturedRequest.EntityName.Should().Be(nameof(User));
        capturedRequest.EntityId.Should().Be(user.Id.ToString());
        capturedRequest.Action.Should().Be("UserUpdated");
        capturedRequest.OldValues.Should().NotBeNull();
        capturedRequest.NewValues.Should().NotBeNull();
        capturedRequest.OldValues!["Fullname"].Should().Be("Alice Doe");
        capturedRequest.NewValues!["Fullname"].Should().Be("Updated Fullname");
        capturedRequest.OldValues!.Keys.Should().OnlyContain(x => x == "Fullname");
        capturedRequest.NewValues!.Keys.Should().OnlyContain(x => x == "Fullname");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_WriteAuditLogWithUsernameOldAndNewValues_When_UsernameChanges()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;
        WriteAuditLogRequest? capturedRequest = null;

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
        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        var auditLogWriterMock = CreateAuditLogWriterMock(request => capturedRequest = request);

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Username = "updateduser" }, ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.OldValues.Should().NotBeNull();
        capturedRequest.NewValues.Should().NotBeNull();
        capturedRequest.OldValues!["Username"].Should().Be("alice");
        capturedRequest.NewValues!["Username"].Should().Be("updateduser");
        capturedRequest.OldValues!.Keys.Should().OnlyContain(x => x == "Username");
        capturedRequest.NewValues!.Keys.Should().OnlyContain(x => x == "Username");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotSendRawPasswordOrPasswordHash_When_PasswordChanges()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;
        WriteAuditLogRequest? capturedRequest = null;

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
        var auditLogWriterMock = CreateAuditLogWriterMock(request => capturedRequest = request);

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Password = "NewPassword123!" }, ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.OldValues.Should().NotBeNull();
        capturedRequest.NewValues.Should().NotBeNull();

        capturedRequest.OldValues!.Keys.Should().NotContain("Password");
        capturedRequest.OldValues.Keys.Should().NotContain("PasswordHash");
        capturedRequest.NewValues!.Keys.Should().NotContain("Password");
        capturedRequest.NewValues.Keys.Should().NotContain("PasswordHash");

        capturedRequest.OldValues.Values.Should().NotContain("NewPassword123!");
        capturedRequest.OldValues.Values.Should().NotContain("hashed-new-password");
        capturedRequest.NewValues.Values.Should().NotContain("NewPassword123!");
        capturedRequest.NewValues.Values.Should().NotContain("hashed-new-password");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_WriteSafePasswordChangedMarker_When_PasswordChanges()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;
        WriteAuditLogRequest? capturedRequest = null;

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
        var auditLogWriterMock = CreateAuditLogWriterMock(request => capturedRequest = request);

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Password = "NewPassword123!" }, ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.OldValues.Should().NotBeNull();
        capturedRequest.NewValues.Should().NotBeNull();
        capturedRequest.OldValues!["PasswordChanged"].Should().Be(false);
        capturedRequest.NewValues!["PasswordChanged"].Should().Be(true);
        capturedRequest.OldValues!.Keys.Should().OnlyContain(x => x == "PasswordChanged");
        capturedRequest.NewValues!.Keys.Should().OnlyContain(x => x == "PasswordChanged");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_WriteOnlySafeChangedFields_When_MultipleFieldsChange()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;
        WriteAuditLogRequest? capturedRequest = null;

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
            .Setup(x => x.Hash("NewPassword123!"))
            .Returns("hashed-new-password");

        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        var auditLogWriterMock = CreateAuditLogWriterMock(request => capturedRequest = request);

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest
        {
            Fullname = "Updated Fullname",
            Username = "updateduser",
            Password = "NewPassword123!"
        }, ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.OldValues.Should().NotBeNull();
        capturedRequest.NewValues.Should().NotBeNull();

        capturedRequest.OldValues!.Should().ContainKey("Fullname");
        capturedRequest.OldValues.Should().ContainKey("Username");
        capturedRequest.OldValues.Should().ContainKey("PasswordChanged");
        capturedRequest.NewValues!.Should().ContainKey("Fullname");
        capturedRequest.NewValues.Should().ContainKey("Username");
        capturedRequest.NewValues.Should().ContainKey("PasswordChanged");

        capturedRequest.OldValues["Fullname"].Should().Be("Alice Doe");
        capturedRequest.NewValues["Fullname"].Should().Be("Updated Fullname");
        capturedRequest.OldValues["Username"].Should().Be("alice");
        capturedRequest.NewValues["Username"].Should().Be("updateduser");
        capturedRequest.OldValues["PasswordChanged"].Should().Be(false);
        capturedRequest.NewValues["PasswordChanged"].Should().Be(true);

        capturedRequest.OldValues.Keys.Should().NotContain("Password");
        capturedRequest.OldValues.Keys.Should().NotContain("PasswordHash");
        capturedRequest.NewValues.Keys.Should().NotContain("Password");
        capturedRequest.NewValues.Keys.Should().NotContain("PasswordHash");
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_UpdateDoesNotChangeAnyValue()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        await sut.UpdateUserAsync(user.Id, new UpdateUserRequest
        {
            Fullname = user.Fullname,
            Username = user.Username
        }, ct);

        // Assert
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_RequestIsNull()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var clockMock = new Mock<IClock>();
        var currentUserMock = new Mock<ICurrentUser>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_RequestIsEmpty()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var clockMock = new Mock<IClock>();
        var currentUserMock = new Mock<ICurrentUser>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(Guid.NewGuid(), new UpdateUserRequest(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_UserIsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(userId, ct))
            .ReturnsAsync((User?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var clockMock = new Mock<IClock>();
        var currentUserMock = new Mock<ICurrentUser>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(userId, new UpdateUserRequest { Fullname = "Updated Fullname" }, ct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_UsernameIsDuplicate()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

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
        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Username = "takenuser" }, ct);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_Should_NotWriteAuditLog_When_PasswordPolicyIsInvalid()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();
        var user = CreateUser(now, actorUserId);
        var ct = new CancellationTokenSource().Token;

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
            .ReturnsAsync(user);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(now);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

        var passwordHasherMock = new Mock<IPasswordHasher>();

        var passwordPolicyMock = new Mock<IPasswordPolicy>();
        passwordPolicyMock
            .Setup(x => x.Validate("weak"))
            .Throws(new DomainException("Password is invalid."));

        var auditLogWriterMock = CreateAuditLogWriterMock();

        var sut = new UserCommandService(
            unitOfWorkMock.Object,
            clockMock.Object,
            currentUserMock.Object,
            passwordHasherMock.Object,
            passwordPolicyMock.Object,
            auditLogWriterMock.Object);

        // Act
        Func<Task> act = async () => await sut.UpdateUserAsync(user.Id, new UpdateUserRequest { Password = "weak" }, ct);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    }

    private static Mock<IAuditLogWriter> CreateAuditLogWriterMock(Action<WriteAuditLogRequest>? capture = null)
    {
        var auditLogWriterMock = new Mock<IAuditLogWriter>();

        var setup = auditLogWriterMock
            .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), It.IsAny<CancellationToken>()));

        if (capture is null)
        {
            setup.Returns(Task.CompletedTask);
        }
        else
        {
            setup
                .Callback<WriteAuditLogRequest, CancellationToken>((request, _) => capture(request))
                .Returns(Task.CompletedTask);
        }

        return auditLogWriterMock;
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