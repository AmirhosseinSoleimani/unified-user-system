using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity;

public class ProfileServiceTests
{
    private static readonly DateTimeOffset T1 =
        new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    [Fact]
    public async Task GetMyProfileAsync_WithAuthenticatedUser_ShouldReturnProfile()
    {
        var user = User.CreateNew(
            "user@example.com",
            "user123",
            "Test User",
            "hashed-password",
            T1,
            null);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUserMock.SetupGet(x => x.UserId).Returns(user.Id);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var sut = new ProfileService(unitOfWorkMock.Object, currentUserMock.Object);

        var result = await sut.GetMyProfileAsync();

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Fullname, result.Fullname);
        Assert.Equal(user.IsActive, result.IsActive);
        Assert.NotNull(result.Roles);
        Assert.Empty(result.Roles);

        userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        userRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_WithoutCurrentUserId_ShouldThrowUnauthorizedAccessException()
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUserMock.SetupGet(x => x.UserId).Returns((Guid?)null);

        var userRepositoryMock = new Mock<IUserRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var sut = new ProfileService(unitOfWorkMock.Object, currentUserMock.Object);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.GetMyProfileAsync());

        Assert.Equal("User is not authenticated.", exception.Message);

        userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        userRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(false);
        currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var userRepositoryMock = new Mock<IUserRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var sut = new ProfileService(unitOfWorkMock.Object, currentUserMock.Object);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.GetMyProfileAsync());

        Assert.Equal("User is not authenticated.", exception.Message);

        userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var currentUserId = Guid.NewGuid();

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var sut = new ProfileService(unitOfWorkMock.Object, currentUserMock.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => sut.GetMyProfileAsync());

        Assert.Equal("User not found.", exception.Message);

        userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(currentUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyProfileAsync_ShouldPassCancellationTokenToRepository()
    {
        var user = User.CreateNew(
            "token@example.com",
            "tokenuser",
            "Token User",
            "hashed-password",
            T1,
            null);

        using var cts = new CancellationTokenSource();

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUserMock.SetupGet(x => x.UserId).Returns(user.Id);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, cts.Token))
            .ReturnsAsync(user);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

        var sut = new ProfileService(unitOfWorkMock.Object, currentUserMock.Object);

        var result = await sut.GetMyProfileAsync(cts.Token);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);

        userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(user.Id, cts.Token),
            Times.Once);
    }
}