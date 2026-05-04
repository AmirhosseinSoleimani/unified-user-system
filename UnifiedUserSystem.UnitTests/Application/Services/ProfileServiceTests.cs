using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services;

public class ProfileServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _unitOfWorkMock.SetupGet(x => x.Users).Returns(_userRepositoryMock.Object);
        _sut = new ProfileService(_unitOfWorkMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task GetMyProfileAsync_Should_ReturnDbBackedProfileResponse_When_AuthenticatedUserExists()
    {
        var userId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        var user = User.CreateNew(
            email: "user@example.com",
            username: "user1",
            fullname: "User One",
            passwordHash: "password-hash",
            nowUtc: nowUtc,
            actorUserId: userId);

        _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(x => x.UserId).Returns(user.Id);

        _userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetMyProfileAsync();

        result.Should().BeEquivalentTo(new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Fullname = user.Fullname,
            IsActive = user.IsActive,
            Roles = Array.Empty<string>()
        });

        _userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyProfileAsync_Should_ThrowUnauthorizedAccessException_When_CurrentUserIsNotAuthenticated()
    {
        _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(false);
        _currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var act = () => _sut.GetMyProfileAsync();

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_Should_ThrowUnauthorizedAccessException_When_CurrentUserIsAuthenticatedButUserIdIsNull()
    {
        _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(x => x.UserId).Returns((Guid?)null);

        var act = () => _sut.GetMyProfileAsync();

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_Should_ThrowKeyNotFoundException_When_RepositoryReturnsNull()
    {
        var userId = Guid.NewGuid();

        _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        _userRepositoryMock
            .Setup(x => x.FindByIdWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.GetMyProfileAsync();

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _userRepositoryMock.Verify(
            x => x.FindByIdWithRolesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void ProfileResponse_Should_NotExposePasswordOrPasswordHashProperties()
    {
        var propertyNames = typeof(ProfileResponse)
            .GetProperties()
            .Select(x => x.Name);

        propertyNames.Should().NotContain("Password");
        propertyNames.Should().NotContain("PasswordHash");
    }
}