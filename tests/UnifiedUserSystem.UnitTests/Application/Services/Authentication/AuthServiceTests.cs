using Microsoft.Extensions.Options;
using Moq;
using UnifiedUserSystem.Application.Services.Authentication;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Validation;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

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
            Assert.False(result!.RequiresMfa);
            Assert.NotNull(result.Tokens);
            Assert.Null(result.MfaChallenge);

            var tokens = result.Tokens!;

            Assert.Equal(user.Id, tokens.Id);
            Assert.Equal(user.Email, tokens.Email);
            Assert.Equal(user.Username, tokens.Username);
            Assert.Equal(user.FirstName, tokens.FirstName);
            Assert.Equal(user.LastName, tokens.LastName);
            Assert.Equal(user.PhoneNumber, tokens.PhoneNumber);
            Assert.Equal(user.Fullname, tokens.Fullname);
            Assert.Equal("access-token", tokens.AccessToken);
            Assert.Equal("new-refresh", tokens.RefreshToken);

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

        [Fact]
        public async Task LoginAsync_WhenMfaEnabledAndEmailEnabled_ShouldReturnChallenge_AndNotReturnTokens()
        {
            var f = new Fixture();
            var user = CreateUser();

            MfaChallenge? addedChallenge = null;

            f.SecuritySettingsService
                .Setup(x => x.GetEffectiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SecuritySettingsResponse
                {
                    Id = Guid.NewGuid(),
                    IsMfaEnabled = true,
                    IsOtpEnabled = true,
                    IsEmailOtpEnabled = true,
                    IsPhoneOtpEnabled = false,
                    OtpExpirationMinutes = 5,
                    OtpMaxAttempts = 3,
                    LoginRateLimitPermitLimit = 10,
                    LoginRateLimitWindowSeconds = 60,
                    RefreshTokenRateLimitPermitLimit = 10,
                    RefreshTokenRateLimitWindowSeconds = 60
                });

            f.Users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            f.Hasher.Setup(x => x.Verify("Password1!", user.PasswordHash)).Returns(true);
            f.OtpGenerator.Setup(x => x.Generate(6)).Returns("123456");
            f.OtpHasher.Setup(x => x.Hash("123456", It.IsAny<Guid>())).Returns("otp-hash");

            f.MfaChallenges
                .Setup(x => x.Add(It.IsAny<MfaChallenge>()))
                .Callback<MfaChallenge>(x => addedChallenge = x);

            var result = await f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "user@example.com",
                Password = "Password1!",
                MfaChannel = "Email"
            });

            Assert.NotNull(result);
            Assert.True(result!.RequiresMfa);
            Assert.Null(result.Tokens);
            Assert.NotNull(result.MfaChallenge);
            Assert.Equal("Email", result.MfaChallenge!.Channel);
            Assert.Equal(new[] { "Email" }, result.MfaChallenge.AvailableChannels);

            Assert.NotNull(addedChallenge);
            Assert.Equal(user.Id, addedChallenge!.UserId);
            Assert.Equal(MfaChannel.Email, addedChallenge.Channel);
            Assert.Equal("otp-hash", addedChallenge.OtpHash);

            f.EmailOtpSender.Verify(
                x => x.SendAsync(user, "123456", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
                Times.Once);

            f.SmsOtpSender.Verify(
                x => x.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
                Times.Never);

            f.RefreshTokenSessions.Verify(x => x.Add(It.IsAny<RefreshTokenSession>()), Times.Never);
            f.Jwt.Verify(x => x.CreateAccessToken(It.IsAny<User>()), Times.Never);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenPhoneChannelIsDisabled_ShouldThrow()
        {
            var f = new Fixture();
            var user = CreateUser();

            f.SecuritySettingsService
                .Setup(x => x.GetEffectiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SecuritySettingsResponse
                {
                    Id = Guid.NewGuid(),
                    IsMfaEnabled = true,
                    IsOtpEnabled = true,
                    IsEmailOtpEnabled = true,
                    IsPhoneOtpEnabled = false,
                    OtpExpirationMinutes = 5,
                    OtpMaxAttempts = 3,
                    LoginRateLimitPermitLimit = 10,
                    LoginRateLimitWindowSeconds = 60,
                    RefreshTokenRateLimitPermitLimit = 10,
                    RefreshTokenRateLimitWindowSeconds = 60
                });

            f.Users.Setup(x => x.FindEmailOrUsernameAsync("user@example.com")).ReturnsAsync(user);
            f.Hasher.Setup(x => x.Verify("Password1!", user.PasswordHash)).Returns(true);

            var act = () => f.Sut.LoginAsync(new LoginRequest
            {
                EmailOrUsername = "user@example.com",
                Password = "Password1!",
                MfaChannel = "Phone"
            });

            var ex = await Assert.ThrowsAsync<DomainException>(act);
            Assert.Equal("MFA channel is disabled.", ex.Message);

            f.MfaChallenges.Verify(x => x.Add(It.IsAny<MfaChallenge>()), Times.Never);
            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        [Fact]
        public async Task VerifyMfaAsync_WithCorrectOtp_ShouldReturnTokens_AndMarkChallengeUsed()
        {
            var f = new Fixture();
            var user = CreateUser();

            var challenge = MfaChallenge.Create(
                user.Id,
                MfaChannel.Email,
                "otp-hash",
                Now,
                Now.AddMinutes(5),
                3,
                user.Id);

            SetPrivateProperty(challenge, "User", user);

            f.SecuritySettingsService
                .Setup(x => x.GetEffectiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SecuritySettingsResponse
                {
                    Id = Guid.NewGuid(),
                    IsMfaEnabled = true,
                    IsOtpEnabled = true,
                    IsEmailOtpEnabled = true,
                    IsPhoneOtpEnabled = true,
                    OtpExpirationMinutes = 5,
                    OtpMaxAttempts = 3,
                    LoginRateLimitPermitLimit = 10,
                    LoginRateLimitWindowSeconds = 60,
                    RefreshTokenRateLimitPermitLimit = 10,
                    RefreshTokenRateLimitWindowSeconds = 60
                });

            f.MfaChallenges
                .Setup(x => x.FindByIdAsync(challenge.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(challenge);

            f.OtpHasher
                .Setup(x => x.Verify("123456", challenge.Id, "otp-hash"))
                .Returns(true);

            f.RefreshTokens.Setup(x => x.GenerateToken()).Returns("refresh-token");
            f.RefreshTokens.Setup(x => x.HashToken("refresh-token")).Returns("refresh-token-hash");
            f.RefreshTokens.Setup(x => x.GetExpiresAtUtc(Now)).Returns(Now.AddDays(7));
            f.Jwt.Setup(x => x.CreateAccessToken(user)).Returns("access-token");

            var result = await f.Sut.VerifyMfaAsync(new VerifyMfaRequest
            {
                ChallengeId = challenge.Id,
                OtpCode = "123456"
            });

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);

            Assert.True(challenge.IsUsed);
            Assert.Equal(1, challenge.AttemptCount);
            Assert.NotNull(challenge.VerifiedAt);

            f.RefreshTokenSessions.Verify(x => x.Add(It.Is<RefreshTokenSession>(s =>
                s.UserId == user.Id &&
                s.RefreshTokenHash == "refresh-token-hash")), Times.Once);

            f.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static void SetPrivateProperty<TValue>(object entity, string propertyName, TValue value)
        {
            entity.GetType()
                .GetProperty(propertyName)!
                .SetValue(entity, value);
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
            public Mock<ISecuritySettingsService> SecuritySettingsService { get; } = new();
            public Mock<IOtpGenerator> OtpGenerator { get; } = new();
            public Mock<IOtpHasher> OtpHasher { get; } = new();
            public Mock<IEmailOtpSender> EmailOtpSender { get; } = new();
            public Mock<ISmsOtpSender> SmsOtpSender { get; } = new();
            public Mock<IMfaChallengeRepository> MfaChallenges { get; } = new();

            public AuthService Sut { get; }

            public Fixture()
            {
                Uow.SetupGet(x => x.Users).Returns(Users.Object);
                Uow.SetupGet(x => x.Roles).Returns(Roles.Object);
                Uow.SetupGet(x => x.RefreshTokenSessions).Returns(RefreshTokenSessions.Object);
                Uow.SetupGet(x => x.MfaChallenges).Returns(MfaChallenges.Object);
                Clock.SetupGet(x => x.Utcnow).Returns(Now);

                ClientContext.SetupGet(x => x.DeviceName).Returns("device");
                ClientContext.SetupGet(x => x.UserAgent).Returns("agent");
                ClientContext.SetupGet(x => x.IpAddress).Returns("127.0.0.1");
                ClientContext.SetupGet(x => x.ClientId).Returns("client");

                AuthProtection
                    .Setup(x => x.CheckAsync(It.IsAny<string>(), ClientContext.Object, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(AuthProtectionCheckResult.Allow());

                SecuritySettingsService.Setup(x => x.GetEffectiveAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new SecuritySettingsResponse
                    {
                        Id = Guid.NewGuid(),
                        IsMfaEnabled = false,
                        IsOtpEnabled = true,
                        IsEmailOtpEnabled = true,
                        IsPhoneOtpEnabled = true,
                        OtpExpirationMinutes = 5,
                        OtpMaxAttempts = 5,
                        LoginRateLimitPermitLimit = 10,
                        LoginRateLimitWindowSeconds = 60,
                        RefreshTokenRateLimitPermitLimit = 10,
                        RefreshTokenRateLimitWindowSeconds = 60
                    });
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
                    AuthProtection.Object,
                    SecuritySettingsService.Object,
                    OtpGenerator.Object,
                    OtpHasher.Object,
                    EmailOtpSender.Object,
                    SmsOtpSender.Object,
                    global::Microsoft.Extensions.Options.Options.Create(new MfaOptions
                    {
                        OtpLength = 6
                    }
                ));
            }
        }
    }
}