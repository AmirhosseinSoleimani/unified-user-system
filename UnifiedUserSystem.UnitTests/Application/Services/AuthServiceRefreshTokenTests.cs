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
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.UnitTests.Application.Services
{
    public class AuthServiceRefreshTokenTests
    {
        private readonly DateTimeOffset _now = new(2026, 06, 01, 10, 00, 00, TimeSpan.Zero);
        private readonly CancellationToken _ct = CancellationToken.None;

        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IRoleRepository> _roles = new();
        private readonly Mock<IRefreshTokenSessionRepository> _sessions = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly Mock<IUserBusiness> _business = new();
        private readonly Mock<IJwtTokenService> _jwt = new();
        private readonly Mock<IRefreshTokenService> _refreshTokens = new();
        private readonly Mock<IClock> _clock = new();
        private readonly Mock<ICurrentUser> _currentUser = new();
        private readonly Mock<IClientContext> _clientContext = new();

        public AuthServiceRefreshTokenTests()
        {
            _uow.SetupGet(x => x.Users).Returns(_users.Object);
            _uow.SetupGet(x => x.Roles).Returns(_roles.Object);
            _uow.SetupGet(x => x.RefreshTokenSessions).Returns(_sessions.Object);
            _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _clock.SetupGet(x => x.Utcnow).Returns(_now);
            _jwt.Setup(x => x.CreateAccessToken(It.IsAny<User>())).Returns("access-token");
            _refreshTokens.Setup(x => x.GenerateToken()).Returns("refresh-token");
            _refreshTokens.Setup(x => x.HashToken(It.IsAny<string>())).Returns<string>(x => $"hash:{x}");
            _refreshTokens.Setup(x => x.GetExpiresAtUtc(It.IsAny<DateTimeOffset>())).Returns<DateTimeOffset>(x => x.AddDays(7));

            _clientContext.SetupGet(x => x.DeviceName).Returns("Chrome");
            _clientContext.SetupGet(x => x.UserAgent).Returns("Mozilla/5.0");
            _clientContext.SetupGet(x => x.IpAddress).Returns("127.0.0.1");
            _clientContext.SetupGet(x => x.ClientId).Returns("web");
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUserAndRefreshTokenSession()
        {
            var role = CreateRole();
            RefreshTokenSession? persistedSession = null;

            _roles.Setup(x => x.FindByIdAsync((int)AppRole.User, _ct)).ReturnsAsync(role);
            _users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
            _users.Setup(x => x.UsernameExistsAsync("username")).ReturnsAsync(false);
            _hasher.Setup(x => x.Hash("Password123!")).Returns("hashed-password");
            _sessions.Setup(x => x.Add(It.IsAny<RefreshTokenSession>()))
                .Callback<RefreshTokenSession>(x => persistedSession = x);

            var sut = CreateSut();

            var response = await sut.RegisterAsync(new RegisterRequest
            {
                Email = "user@example.com",
                Username = "username",
                FullName = "Test User",
                Password = "Password123!"
            }, _ct);

            response.AccessToken.Should().Be("access-token");
            response.RefreshToken.Should().Be("refresh-token");
            response.RefreshTokenExpiresAtUtc.Should().Be(_now.AddDays(7));

            persistedSession.Should().NotBeNull();
            persistedSession!.RefreshTokenHash.Should().Be("hash:refresh-token");
            persistedSession.RefreshTokenHash.Should().NotBe("refresh-token");
            persistedSession.DeviceName.Should().Be("Chrome");

            _sessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldCreateRefreshTokenSession()
        {
            var user = CreateUser();
            RefreshTokenSession? persistedSession = null;

            _users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            _hasher.Setup(x => x.Verify("Password123!", user.PasswordHash)).Returns(true);
            _sessions.Setup(x => x.Add(It.IsAny<RefreshTokenSession>()))
                .Callback<RefreshTokenSession>(x => persistedSession = x);

            var sut = CreateSut();

            var response = await sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "user@example.com",
                Password = "Password123!"
            }, _ct);

            response.Should().NotBeNull();
            response!.RefreshToken.Should().Be("refresh-token");
            response.RefreshTokenExpiresAtUtc.Should().Be(_now.AddDays(7));
            persistedSession.Should().NotBeNull();
            persistedSession!.RefreshTokenHash.Should().Be("hash:refresh-token");

            _sessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_ShouldReturnNewTokens()
        {
            var user = CreateUser();
            var currentSession = CreateSession(user.Id, "hash:old-refresh-token");
            RefreshTokenSession? newSession = null;

            _refreshTokens.SetupSequence(x => x.GenerateToken())
                .Returns("new-refresh-token");
            _sessions.Setup(x => x.FindByHashAsync("hash:old-refresh-token", _ct)).ReturnsAsync(currentSession);
            _users.Setup(x => x.FindByIdWithRolesAsync(user.Id, _ct)).ReturnsAsync(user);
            _sessions.Setup(x => x.Add(It.IsAny<RefreshTokenSession>()))
                .Callback<RefreshTokenSession>(x => newSession = x);

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest
            {
                RefreshToken = "old-refresh-token"
            }, _ct);

            response.Should().NotBeNull();
            response!.AccessToken.Should().Be("access-token");
            response.RefreshToken.Should().Be("new-refresh-token");
            newSession.Should().NotBeNull();
            newSession!.RefreshTokenHash.Should().Be("hash:new-refresh-token");
        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_ShouldRevokeOldSession()
        {
            var user = CreateUser();
            var currentSession = CreateSession(user.Id, "hash:old-refresh-token");

            _refreshTokens.Setup(x => x.GenerateToken()).Returns("new-refresh-token");
            _sessions.Setup(x => x.FindByHashAsync("hash:old-refresh-token", _ct)).ReturnsAsync(currentSession);
            _users.Setup(x => x.FindByIdWithRolesAsync(user.Id, _ct)).ReturnsAsync(user);

            var sut = CreateSut();

            await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "old-refresh-token" }, _ct);

            currentSession.IsRevoked.Should().BeTrue();
            currentSession.RevokedAtUtc.Should().Be(_now);
            currentSession.ReplacedBySessionId.Should().NotBeNull();
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithRevokedToken_ShouldMarkReuseDetected()
        {
            var user = CreateUser();
            var revokedSession = CreateSession(user.Id, "hash:old-refresh-token");
            revokedSession.Revoke(_now.AddMinutes(-1), user.Id);

            _sessions.Setup(x => x.FindByHashAsync("hash:old-refresh-token", _ct)).ReturnsAsync(revokedSession);
            _sessions.Setup(x => x.ListByUserIdAsync(user.Id, _ct)).ReturnsAsync(new[] { revokedSession });

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "old-refresh-token" }, _ct);

            response.Should().BeNull();
            revokedSession.ReuseDetectedAtUtc.Should().Be(_now);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithRevokedToken_ShouldRevokeAffectedSessionChain()
        {
            var user = CreateUser();
            var reusedSession = CreateSession(user.Id, "hash:old-refresh-token");
            var replacementSession = CreateSession(user.Id, "hash:replacement-refresh-token");
            reusedSession.Rotate(replacementSession.Id, _now.AddMinutes(-2), user.Id);
            reusedSession.Revoke(_now.AddMinutes(-1), user.Id);

            _sessions.Setup(x => x.FindByHashAsync("hash:old-refresh-token", _ct)).ReturnsAsync(reusedSession);
            _sessions.Setup(x => x.ListByUserIdAsync(user.Id, _ct)).ReturnsAsync(new[] { reusedSession, replacementSession });

            var sut = CreateSut();

            await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "old-refresh-token" }, _ct);

            replacementSession.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAsync_WithExpiredToken_ShouldReturnNull()
        {
            var user = CreateUser();
            var expiredSession = CreateSession(user.Id, "hash:expired-refresh-token", expiresAtUtc: _now.AddMinutes(-1));

            _sessions.Setup(x => x.FindByHashAsync("hash:expired-refresh-token", _ct)).ReturnsAsync(expiredSession);

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "expired-refresh-token" }, _ct);

            response.Should().BeNull();
            _sessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Never);
        }

        [Fact]
        public async Task RefreshAsync_WithInvalidToken_ShouldReturnNull()
        {
            _sessions.Setup(x => x.FindByHashAsync("hash:invalid-refresh-token", _ct))
                .ReturnsAsync((RefreshTokenSession?)null);

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "invalid-refresh-token" }, _ct);

            response.Should().BeNull();
            _refreshTokens.Verify(x => x.HashToken("invalid-refresh-token"), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithInactiveUser_ShouldReturnNull()
        {
            var user = CreateUser();
            user.Deactive(_now, user.Id);
            var session = CreateSession(user.Id, "hash:refresh-token");

            _sessions.Setup(x => x.FindByHashAsync("hash:refresh-token", _ct)).ReturnsAsync(session);
            _users.Setup(x => x.FindByIdWithRolesAsync(user.Id, _ct)).ReturnsAsync(user);

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "refresh-token" }, _ct);

            response.Should().BeNull();
        }

        [Fact]
        public async Task RefreshAsync_ShouldReturnDifferentRefreshToken()
        {
            var user = CreateUser();
            var session = CreateSession(user.Id, "hash:old-refresh-token");
            _refreshTokens.Setup(x => x.GenerateToken()).Returns("new-refresh-token");
            _sessions.Setup(x => x.FindByHashAsync("hash:old-refresh-token", _ct)).ReturnsAsync(session);
            _users.Setup(x => x.FindByIdWithRolesAsync(user.Id, _ct)).ReturnsAsync(user);

            var sut = CreateSut();

            var response = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "old-refresh-token" }, _ct);

            response!.RefreshToken.Should().NotBe("old-refresh-token");
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_ShouldRevokeSessionAndReturnTrue()
        {
            var user = CreateUser();
            var session = CreateSession(user.Id, "hash:refresh-token");

            _sessions.Setup(x => x.FindByHashAsync("hash:refresh-token", _ct)).ReturnsAsync(session);

            var sut = CreateSut();

            var result = await sut.LogoutAsync(user.Id, new LogoutRequest { RefreshToken = "refresh-token" }, _ct);

            result.Should().BeTrue();
            session.IsRevoked.Should().BeTrue();
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WithInvalidToken_ShouldReturnFalse()
        {
            var user = CreateUser();

            _sessions.Setup(x => x.FindByHashAsync("hash:invalid-refresh-token", _ct))
                .ReturnsAsync((RefreshTokenSession?)null);

            var sut = CreateSut();

            var result = await sut.LogoutAsync(user.Id, new LogoutRequest { RefreshToken = "invalid-refresh-token" }, _ct);

            result.Should().BeFalse();
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WithExpiredToken_ShouldReturnFalse()
        {
            var user = CreateUser();
            var session = CreateSession(user.Id, "hash:expired-refresh-token", expiresAtUtc: _now.AddMinutes(-1));

            _sessions.Setup(x => x.FindByHashAsync("hash:expired-refresh-token", _ct)).ReturnsAsync(session);

            var sut = CreateSut();

            var result = await sut.LogoutAsync(user.Id, new LogoutRequest { RefreshToken = "expired-refresh-token" }, _ct);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_WithDifferentUserToken_ShouldReturnFalse()
        {
            var currentUserId = Guid.NewGuid();
            var otherUserSession = CreateSession(Guid.NewGuid(), "hash:refresh-token");

            _sessions.Setup(x => x.FindByHashAsync("hash:refresh-token", _ct)).ReturnsAsync(otherUserSession);

            var sut = CreateSut();

            var result = await sut.LogoutAsync(currentUserId, new LogoutRequest { RefreshToken = "refresh-token" }, _ct);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeAllSessionsAsync_ShouldRevokeOnlyActiveSessionsForUser()
        {
            var user = CreateUser();
            var activeSession = CreateSession(user.Id, "hash:active-token");
            var otherUserSession = CreateSession(Guid.NewGuid(), "hash:other-token");

            _sessions.Setup(x => x.ListActiveByUserIdAsync(user.Id, _now, _ct))
                .ReturnsAsync(new[] { activeSession });

            var sut = CreateSut();

            await sut.RevokeAllSessionsAsync(user.Id, _ct);

            activeSession.IsRevoked.Should().BeTrue();
            otherUserSession.IsRevoked.Should().BeFalse();
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private AuthService CreateSut()
        {
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

        private User CreateUser()
        {
            return User.CreateNew(
                "user@example.com",
                "username",
                "Test User",
                "hashed-password",
                _now,
                Guid.NewGuid());
        }

        private Role CreateRole()
        {
            var role = Role.Create("user", "user", _now, Guid.NewGuid());
            SetProtectedId(role, (int)AppRole.User);
            return role;
        }

        private RefreshTokenSession CreateSession(
            Guid userId,
            string refreshTokenHash,
            DateTimeOffset? expiresAtUtc = null)
        {
            return RefreshTokenSession.Create(
                userId,
                refreshTokenHash,
                _now.AddMinutes(-5),
                expiresAtUtc ?? _now.AddDays(7),
                null,
                null,
                null,
                null,
                userId);
        }

        private static void SetProtectedId<TKey>(Entity<TKey> entity, TKey id)
            where TKey : struct
        {
            typeof(Entity<TKey>)
                .GetProperty(nameof(Entity<TKey>.Id))!
                .SetValue(entity, id);
        }
    }
}