using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.UnitTests.TestHelpers;

namespace UnifiedUserSystem.UnitTests.Application.Services
{
    public class AuthServiceLoginProtectionTests
    {
        private readonly DateTimeOffset _now = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        private readonly CancellationToken _ct = CancellationToken.None;

        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IRefreshTokenSessionRepository> _sessions = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly Mock<IUserBusiness> _business = new();
        private readonly Mock<IJwtTokenService> _jwt = new();
        private readonly Mock<IRefreshTokenService> _refreshTokens = new();
        private readonly Mock<IClock> _clock = new();
        private readonly Mock<ICurrentUser> _currentUser = new();
        private readonly Mock<IAuthProtectionService> _authProtection = new();
        private readonly TestClientContext _clientContext = new();

        public AuthServiceLoginProtectionTests()
        {
            _uow.SetupGet(x => x.Users).Returns(_users.Object);
            _uow.SetupGet(x => x.RefreshTokenSessions).Returns(_sessions.Object);
            _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _clock.SetupGet(x => x.Utcnow).Returns(_now);
            _jwt.Setup(x => x.CreateAccessToken(It.IsAny<User>())).Returns("access-token");
            _refreshTokens.Setup(x => x.GenerateToken()).Returns("refresh-token");
            _refreshTokens.Setup(x => x.HashToken(It.IsAny<string>())).Returns<string>(x => $"hash:{x}");
            _refreshTokens.Setup(x => x.GetExpiresAtUtc(It.IsAny<DateTimeOffset>())).Returns<DateTimeOffset>(x => x.AddDays(7));

            _authProtection
                .Setup(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<IClientContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AuthProtectionCheckResult.Allow());
        }

        [Fact]
        public async Task LoginAsync_WithUnknownUser_ShouldRecordFailedAttempt()
        {
            _users.Setup(x => x.FindEmailOrUsernameAsync("missing@example.com")).ReturnsAsync((User?)null);

            var result = await CreateSut().LoginAsync(Request("missing@example.com"), _ct);

            result.Should().BeNull();
            _authProtection.Verify(x => x.RecordFailureAsync("missing@example.com", _clientContext, _ct), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldRecordFailedAttempt()
        {
            var user = CreateUser();
            _users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            _hasher.Setup(x => x.Verify("bad-password", user.PasswordHash)).Returns(false);

            var result = await CreateSut().LoginAsync(Request("user@example.com", "bad-password"), _ct);

            result.Should().BeNull();
            _authProtection.Verify(x => x.RecordFailureAsync("user@example.com", _clientContext, _ct), Times.Once);
        }

        [Theory]
        [InlineData("locked-identity@example.com")]
        [InlineData("locked-client@example.com")]
        [InlineData("cooldown@example.com")]
        public async Task LoginAsync_WithBlockedLogin_ShouldReturnSanitizedFailure(string identifier)
        {
            _authProtection
                .Setup(x => x.CheckAsync(identifier, _clientContext, _ct))
                .ReturnsAsync(AuthProtectionCheckResult.Block());

            var result = await CreateSut().LoginAsync(Request(identifier), _ct);

            result.Should().BeNull();
            _users.Verify(x => x.FindEmailOrUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithLockedIdentity_ShouldReturnSanitizedFailure()
        {
            await LoginAsync_WithBlockedLogin_ShouldReturnSanitizedFailure("locked-identity@example.com");
        }

        [Fact]
        public async Task LoginAsync_WithLockedClient_ShouldReturnSanitizedFailure()
        {
            await LoginAsync_WithBlockedLogin_ShouldReturnSanitizedFailure("locked-client@example.com");
        }

        [Fact]
        public async Task LoginAsync_WithCooldownActive_ShouldReturnSanitizedFailure()
        {
            await LoginAsync_WithBlockedLogin_ShouldReturnSanitizedFailure("cooldown@example.com");
        }

        [Fact]
        public async Task LoginAsync_WithSuccessfulLogin_ShouldResetFailedAttemptState()
        {
            var user = CreateUser();
            _users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            _hasher.Setup(x => x.Verify("Password123!", user.PasswordHash)).Returns(true);

            var result = await CreateSut().LoginAsync(Request("user@example.com"), _ct);

            result.Should().NotBeNull();
            _authProtection.Verify(x => x.ResetAsync("user@example.com", _clientContext, _ct), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithSuccessfulLogin_ShouldStillCreateRefreshTokenSession()
        {
            var user = CreateUser();
            _users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            _hasher.Setup(x => x.Verify("Password123!", user.PasswordHash)).Returns(true);

            await CreateSut().LoginAsync(Request("user@example.com"), _ct);

            _sessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithBlockedLogin_ShouldNotGenerateAccessToken()
        {
            _authProtection
                .Setup(x => x.CheckAsync("user@example.com", _clientContext, _ct))
                .ReturnsAsync(AuthProtectionCheckResult.Block());

            await CreateSut().LoginAsync(Request("user@example.com"), _ct);

            _jwt.Verify(x => x.CreateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithBlockedLogin_ShouldNotCreateRefreshTokenSession()
        {
            _authProtection
                .Setup(x => x.CheckAsync("user@example.com", _clientContext, _ct))
                .ReturnsAsync(AuthProtectionCheckResult.Block());

            await CreateSut().LoginAsync(Request("user@example.com"), _ct);

            _sessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownUser_ShouldNotRevealUserExistence()
        {
            var result = await CreateSut().LoginAsync(Request("missing@example.com"), _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldNotRevealUserExistence()
        {
            await LoginAsync_WithInvalidPassword_ShouldRecordFailedAttempt();
        }

        [Fact]
        public async Task LoginAsync_WithDifferentUsernameCasing_ShouldUseSameIdentityBucket()
        {
            var user = CreateUser();
            _users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            _hasher.Setup(x => x.Verify("bad-password", user.PasswordHash)).Returns(false);

            await CreateSut().LoginAsync(Request(" USER@EXAMPLE.COM ", "bad-password"), _ct);

            _authProtection.Verify(x => x.CheckAsync("user@example.com", _clientContext, _ct), Times.Once);
            _authProtection.Verify(x => x.RecordFailureAsync("user@example.com", _clientContext, _ct), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithDifferentIpAddress_ShouldUseDifferentClientBucket()
        {
            var firstContext = new TestClientContext { IpAddress = "10.0.0.1", ClientId = "web" };
            var secondContext = new TestClientContext { IpAddress = "10.0.0.2", ClientId = "web" };

            var firstSut = CreateSut(firstContext);
            var secondSut = CreateSut(secondContext);

            await firstSut.LoginAsync(Request("missing@example.com"), _ct);
            await secondSut.LoginAsync(Request("missing@example.com"), _ct);

            _authProtection.Verify(x => x.RecordFailureAsync("missing@example.com", firstContext, _ct), Times.Once);
            _authProtection.Verify(x => x.RecordFailureAsync("missing@example.com", secondContext, _ct), Times.Once);
        }

        private AuthService CreateSut(IClientContext? clientContext = null)
        {
            return new AuthService(
                _uow.Object,
                _hasher.Object,
                _business.Object,
                _jwt.Object,
                _refreshTokens.Object,
                _clock.Object,
                _currentUser.Object,
                clientContext ?? _clientContext,
                _authProtection.Object);
        }

        private static LoginRequest Request(string identifier, string password = "Password123!")
        {
            return new LoginRequest
            {
                EmailOrUsername = identifier,
                Password = password
            };
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
    }
}