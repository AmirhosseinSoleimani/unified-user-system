using Moq;
using UnifiedUserSystem.Application.Services.Authentication;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Application.Validation;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services
{
    public class AuthServiceTests
    {
        private static readonly DateTimeOffset Now = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

        [Fact]
        public async Task RegisterAsync_WithValidRequest_ShouldCreateUser_HashPassword_AssignDefaultRole_Save_AndReturnAuthResponse()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();

            User? addedUser = null;
            RefreshTokenSession? addedSession = null;

            var role = Role.Create("user", "User", Now, null);
            SetId(role, 1);

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
            f.Users.Setup(x => x.UsernameExistsAsync("user123")).ReturnsAsync(false);
            f.Roles.Setup(x => x.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(role);

            f.Users
                .Setup(x => x.Add(It.IsAny<User>()))
                .Callback<User>(u => addedUser = u);

            f.RefreshTokenSessions
                .Setup(x => x.Add(It.IsAny<RefreshTokenSession>()))
                .Callback<RefreshTokenSession>(s => addedSession = s);

            f.Hasher.Setup(x => x.Hash("Password1!")).Returns("HASHED");
            f.RefreshTokens.Setup(x => x.GenerateToken()).Returns("refresh-token");
            f.RefreshTokens.Setup(x => x.HashToken("refresh-token")).Returns("refresh-token-hash");
            f.RefreshTokens.Setup(x => x.GetExpiresAtUtc(Now)).Returns(Now.AddDays(7));
            f.Jwt.Setup(x => x.CreateAccessToken(It.IsAny<User>())).Returns("access-token");

            var result = await f.Sut.RegisterAsync(req);

            Assert.NotNull(addedUser);
            Assert.Equal("user@example.com", addedUser!.Email);
            Assert.Equal("user123", addedUser.Username);
            Assert.Equal("Full", addedUser.FirstName);
            Assert.Equal("Name", addedUser.LastName);
            Assert.Equal("+989123456789", addedUser.PhoneNumber);
            Assert.Equal("Full Name", addedUser.Fullname);
            Assert.Equal("HASHED", addedUser.PasswordHash);
            Assert.DoesNotContain("Password1!", addedUser.PasswordHash);
            Assert.Single(addedUser.UserRoles);
            Assert.Equal(1, addedUser.UserRoles.First().RoleId);

            Assert.NotNull(addedSession);
            Assert.Equal(addedUser.Id, addedSession!.UserId);
            Assert.Equal("refresh-token-hash", addedSession.RefreshTokenHash);

            Assert.Equal(addedUser.Id, result.Id);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("user123", result.Username);
            Assert.Equal("Full", result.FirstName);
            Assert.Equal("Name", result.LastName);
            Assert.Equal("+989123456789", result.PhoneNumber);
            Assert.Equal("Full Name", result.Fullname);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(Now.AddDays(7), result.RefreshTokenExpiresAtUtc);

            f.RegistrationValidator.Verify(x => x.Validate(req), Times.Once);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldThrow_AndNotCreateUser()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.RegisterAsync(req));

            Assert.Equal("Email already exists.", ex.Message);
            f.Users.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateUsername_ShouldThrow_AndNotCreateUser()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
            f.Users.Setup(x => x.UsernameExistsAsync("user123")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.RegisterAsync(req));

            Assert.Equal("Username already exists.", ex.Message);
            f.Users.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenDefaultRoleMissing_ShouldThrow_AndNotSave()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
            f.Users.Setup(x => x.UsernameExistsAsync("user123")).ReturnsAsync(false);
            f.Roles.Setup(x => x.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.RegisterAsync(req));

            Assert.Equal("Default role not found. Seed roles first.", ex.Message);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldNormalizeEmailBeforeCheckingDuplicates()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();
            req.Email = " USER@Example.COM ";

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.RegisterAsync(req));

            f.Users.Verify(x => x.EmailExistsAsync("user@example.com"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldNormalizeUsernameBeforeCheckingDuplicates()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();
            req.Username = " user123 ";

            f.Users.Setup(x => x.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
            f.Users.Setup(x => x.UsernameExistsAsync("user123")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.RegisterAsync(req));

            f.Users.Verify(x => x.UsernameExistsAsync("user123"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidRequest_ShouldThrowFromRegistrationValidator()
        {
            var f = new Fixture();
            var req = ValidRegisterRequest();

            f.RegistrationValidator
                .Setup(x => x.Validate(req))
                .Throws(new DomainException("invalid"));

            await Assert.ThrowsAsync<DomainException>(() => f.Sut.RegisterAsync(req));

            f.Users.Verify(x => x.EmailExistsAsync(It.IsAny<string>()), Times.Never);
            f.Users.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithValidEmail_ShouldReturnAuthResponse_VerifyPassword_CreateRefreshSession_Save_AndResetProtection()
        {
            var f = new Fixture();
            var user = CreateUser();

            f.Users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            f.Hasher.Setup(x => x.Verify("Password1!", user.PasswordHash)).Returns(true);
            f.RefreshTokens.Setup(x => x.GenerateToken()).Returns("new-refresh");
            f.RefreshTokens.Setup(x => x.HashToken("new-refresh")).Returns("new-refresh-hash");
            f.RefreshTokens.Setup(x => x.GetExpiresAtUtc(Now)).Returns(Now.AddDays(7));
            f.Jwt.Setup(x => x.CreateAccessToken(user)).Returns("access-token");

            var result = await f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = " USER@Example.COM ",
                Password = "Password1!"
            });

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.PhoneNumber, result.PhoneNumber);
            Assert.Equal(user.Fullname, result.Fullname);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("new-refresh", result.RefreshToken);

            f.LoginValidator.Verify(x => x.Validate(It.IsAny<LoginRequest>()), Times.Once);
            f.Hasher.Verify(x => x.Verify("Password1!", user.PasswordHash), Times.Once);

            f.RefreshTokenSessions.Verify(x => x.Add(It.Is<RefreshTokenSession>(s =>
                s.UserId == user.Id &&
                s.RefreshTokenHash == "new-refresh-hash")), Times.Once);

            f.AuthProtection.Verify(x => x.ResetAsync(
                "user@example.com",
                f.ClientContext.Object,
                It.IsAny<CancellationToken>()), Times.Once);

            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownUser_ShouldReturnNull_RecordFailure_AndNotRevealExistence()
        {
            var f = new Fixture();

            f.Users.Setup(x => x.FindEmailOrUsernameAsync("missing@example.com")).ReturnsAsync((User?)null);

            var result = await f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "missing@example.com",
                Password = "Password1!"
            });

            Assert.Null(result);

            f.AuthProtection.Verify(x => x.RecordFailureAsync(
                "missing@example.com",
                f.ClientContext.Object,
                It.IsAny<CancellationToken>()), Times.Once);

            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull_RecordFailure_AndNotSave()
        {
            var f = new Fixture();
            var user = CreateUser();

            f.Users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            f.Hasher.Setup(x => x.Verify("wrong", user.PasswordHash)).Returns(false);

            var result = await f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "user@example.com",
                Password = "wrong"
            });

            Assert.Null(result);

            f.AuthProtection.Verify(x => x.RecordFailureAsync(
                "user@example.com",
                f.ClientContext.Object,
                It.IsAny<CancellationToken>()), Times.Once);

            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenProtectionBlocks_ShouldReturnNull_AndNotQueryUser()
        {
            var f = new Fixture();

            f.AuthProtection
                .Setup(x => x.CheckAsync("user@example.com", f.ClientContext.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(AuthProtectionCheckResult.Block());

            var result = await f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "user@example.com",
                Password = "Password1!"
            });

            Assert.Null(result);
            f.Users.Verify(x => x.FindEmailOrUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        private static RegisterRequest ValidRegisterRequest()
        {
            return new RegisterRequest
            {
                Email = "user@example.com",
                Username = "user123",
                FirstName = "Full",
                LastName = "Name",
                PhoneNumber = "+989123456789",
                FullName = "Full Name",
                Password = "Password1!"
            };
        }

        private static User CreateUser()
        {
            return User.CreateNew(
                email: "user@example.com",
                username: "user123",
                firstName: "Full",
                lastName: "Name",
                phoneNumber: "+989123456789",
                passwordHash: "HASHED",
                nowUtc: Now,
                actorUserId: Guid.NewGuid());
        }

        private static void SetId<T>(T entity, int id)
        {
            typeof(T).BaseType!.BaseType!.GetProperty("Id")!.SetValue(entity, id);
        }

        private sealed class Fixture
        {
            public Mock<IUnitOfWork> Uow { get; } = new();
            public Mock<IUserRepository> Users { get; } = new();
            public Mock<IRoleRepository> Roles { get; } = new();
            public Mock<IRefreshTokenSessionRepository> RefreshTokenSessions { get; } = new();
            public Mock<IPasswordHasher> Hasher { get; } = new();
            public Mock<IRegistrationRequestValidator> RegistrationValidator { get; } = new();
            public Mock<ILoginRequestValidator> LoginValidator { get; } = new();
            public Mock<IJwtTokenService> Jwt { get; } = new();
            public Mock<IRefreshTokenService> RefreshTokens { get; } = new();
            public Mock<IClock> Clock { get; } = new();
            public Mock<ICurrentUser> CurrentUser { get; } = new();
            public Mock<IClientContext> ClientContext { get; } = new();
            public Mock<IAuthProtectionService> AuthProtection { get; } = new();

            public AuthService Sut { get; }

            public Fixture()
            {
                Uow.SetupGet(x => x.Users).Returns(Users.Object);
                Uow.SetupGet(x => x.Roles).Returns(Roles.Object);
                Uow.SetupGet(x => x.RefreshTokenSessions).Returns(RefreshTokenSessions.Object);

                Clock.SetupGet(x => x.Utcnow).Returns(Now);

                ClientContext.SetupGet(x => x.DeviceName).Returns("device");
                ClientContext.SetupGet(x => x.UserAgent).Returns("agent");
                ClientContext.SetupGet(x => x.IpAddress).Returns("127.0.0.1");
                ClientContext.SetupGet(x => x.ClientId).Returns("client");

                AuthProtection
                    .Setup(x => x.CheckAsync(It.IsAny<string>(), ClientContext.Object, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(AuthProtectionCheckResult.Allow());

                Sut = new AuthService(
                    Uow.Object,
                    Hasher.Object,
                    RegistrationValidator.Object,
                    LoginValidator.Object,
                    Jwt.Object,
                    RefreshTokens.Object,
                    Clock.Object,
                    CurrentUser.Object,
                    ClientContext.Object,
                    AuthProtection.Object);
            }
        }
    }
}