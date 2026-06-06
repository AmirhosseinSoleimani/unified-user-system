using System.Reflection;
using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application.Services;

public class AuthServiceRegistrationTests
{
    private static readonly DateTimeOffset Now = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<IRoleRepository> _roles = new(MockBehavior.Strict);
    private readonly Mock<IRefreshTokenSessionRepository> _refreshTokenSessions = new(MockBehavior.Strict);
    private readonly Mock<IPasswordHasher> _hasher = new(MockBehavior.Strict);
    private readonly Mock<IUserBusiness> _business = new(MockBehavior.Strict);
    private readonly Mock<IJwtTokenService> _jwt = new(MockBehavior.Strict);
    private readonly Mock<IRefreshTokenService> _refreshTokens = new(MockBehavior.Strict);
    private readonly Mock<IClock> _clock = new(MockBehavior.Strict);
    private readonly Mock<ICurrentUser> _currentUser = new(MockBehavior.Strict);
    private readonly Mock<IClientContext> _clientContext = new(MockBehavior.Strict);

    [Fact]
    public async Task RegisterAsync_ShouldAssignResolvedDefaultRoleId()
    {
        User? savedUser = null;
        var sut = CreateSut(RoleWithId(42, "Resolved Default Role"), user => savedUser = user);

        await sut.RegisterAsync(ValidRequest());

        savedUser.Should().NotBeNull();
        savedUser!.UserRoles.Should().ContainSingle();
        savedUser.UserRoles.Single().RoleId.Should().Be(42);
    }

    [Fact]
    public async Task RegisterAsync_ShouldNotHardCodeRoleIdOne()
    {
        User? savedUser = null;
        var sut = CreateSut(RoleWithId(42, "Resolved Default Role"), user => savedUser = user);

        await sut.RegisterAsync(ValidRequest());

        savedUser!.UserRoles.Single().RoleId.Should().NotBe(1);
    }

    [Fact]
    public async Task RegisterAsync_WhenDefaultRoleMissing_ShouldFailClearly()
    {
        var sut = CreateSut(defaultRole: null, onUserAdded: _ => { });

        var act = () => sut.RegisterAsync(ValidRequest());

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Default role not found*");

        _users.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _refreshTokenSessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithResolvedDefaultRole_ShouldReturnRoleNameInResponse()
    {
        var sut = CreateSut(RoleWithId(42, "Resolved Default Role"), _ => { });

        var response = await sut.RegisterAsync(ValidRequest());

        response.Roles.Should().ContainSingle()
            .Which.Should().Be("Resolved Default Role");
    }

    [Fact]
    public async Task RegisterAsync_WithResolvedDefaultRole_ShouldSaveUserWithExpectedUserRole()
    {
        User? savedUser = null;
        var sut = CreateSut(RoleWithId(42, "Resolved Default Role"), user => savedUser = user);

        await sut.RegisterAsync(ValidRequest());

        savedUser.Should().NotBeNull();
        savedUser!.UserRoles.Should().ContainSingle(x => x.RoleId == 42);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private AuthService CreateSut(Role? defaultRole, Action<User> onUserAdded)
    {
        _business.Setup(x => x.ValidateRegister(It.IsAny<RegisterRequest>()));

        _users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
        _users.Setup(x => x.UsernameExistsAsync("testuser")).ReturnsAsync(false);
        _users.Setup(x => x.Add(It.IsAny<User>())).Callback(onUserAdded);

        _roles.Setup(x => x.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultRole);

        _refreshTokenSessions.Setup(x => x.Add(It.IsAny<RefreshTokenSession>()));

        _hasher.Setup(x => x.Hash("Password123!")).Returns("hashed-password");
        _jwt.Setup(x => x.CreateAccessToken(It.IsAny<User>())).Returns("access-token");

        _refreshTokens.Setup(x => x.GenerateToken()).Returns("refresh-token");
        _refreshTokens.Setup(x => x.HashToken("refresh-token")).Returns("refresh-token-hash");
        _refreshTokens.Setup(x => x.GetExpiresAtUtc(Now)).Returns(Now.AddDays(7));

        _clock.SetupGet(x => x.Utcnow).Returns(Now);

        _clientContext.SetupGet(x => x.DeviceName).Returns("test-device");
        _clientContext.SetupGet(x => x.UserAgent).Returns("test-agent");
        _clientContext.SetupGet(x => x.IpAddress).Returns("127.0.0.1");
        _clientContext.SetupGet(x => x.ClientId).Returns("test-client");

        _uow.SetupGet(x => x.Users).Returns(_users.Object);
        _uow.SetupGet(x => x.Roles).Returns(_roles.Object);
        _uow.SetupGet(x => x.RefreshTokenSessions).Returns(_refreshTokenSessions.Object);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new AuthService(
            _uow.Object,
            _hasher.Object,
            _business.Object,
            _jwt.Object,
            _refreshTokens.Object,
            _clock.Object,
            _currentUser.Object,
            _clientContext.Object);
    }

    private static RegisterRequest ValidRequest() => new()
    {
        Email = "User@Example.com",
        Username = "testuser",
        FullName = "Test User",
        Password = "Password123!"
    };

    private static Role RoleWithId(int id, string name)
    {
        var role = Role.Create("resolved-default-role", name, Now, actorUserId: null);

        typeof(Entity<int>)
            .GetProperty(nameof(Entity<int>.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(role, new object[] { id });

        return role;
    }
}