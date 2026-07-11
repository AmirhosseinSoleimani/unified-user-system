
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UserEntity = UnifiedUserSystem.src.Domain.Identity.Entities.User;


namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserCreationTests
{
    [Fact]
    public void CreateNew_WithValidData_ShouldCreateUser()
    {
        var user = UserTestFactory.Create();

        user.Id.Should().NotBeEmpty();

        user.Email.Should().Be("user@example.com");
        user.Username.Should().Be("test.user");
        user.FirstName.Should().Be("Amirhossein");
        user.LastName.Should().Be("Soleimani");
        user.PhoneNumber.Should().Be("09123456789");
        user.Fullname.Should().Be("Amirhossein Soleimani");
        user.PasswordHash.Should().Be("valid-password-hash");

        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();

        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.RefreshTokenSessions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CreateNew_WithoutActor_ShouldUseCreatedUserAsAuditActor()
    {
        var user = UserTestFactory.Create(actorUserId: null);

        user.CreatedAt.Should().Be(UserTestFactory.CreatedAt);
        user.UpdatedAt.Should().Be(UserTestFactory.CreatedAt);

        user.CreatedByUserId.Should().Be(user.Id);
        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public void CreateNew_WithActor_ShouldUseProvidedActorForAudit()
    {
        var actorId = UserTestFactory.ActorUserId;

        var user = UserTestFactory.Create(actorUserId: actorId);

        user.CreatedByUserId.Should().Be(actorId);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void CreateNew_ShouldGenerateDifferentIdsForDifferentUsers()
    {
        var firstUser = UserTestFactory.Create(
            email: "first@example.com",
            username: "first.user");

        var secondUser = UserTestFactory.Create(
            email: "second@example.com",
            username: "second.user");

        firstUser.Id.Should().NotBe(secondUser.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void CreateNew_WhenEmailMissing_ShouldThrowDomainException(
        string? email)
    {
        var action = () => UserTestFactory.Create(email: email!);

        action.Should()
            .Throw<DomainException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("user@localhost")]
    [InlineData("user example@example.com")]
    [InlineData("user@example")]
    [InlineData("user@@example.com")]
    public void CreateNew_WhenEmailFormatInvalid_ShouldThrowDomainException(
        string email)
    {
        var action = () => UserTestFactory.Create(email: email);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*email format is invalid*");
    }

    [Fact]
    public void CreateNew_WhenEmailExceedsMaximumLength_ShouldThrow()
    {
        var localPart = UserTestFactory.CreateString(UserEntity.EmailMaxLength);
        var email = $"{localPart}@example.com";

        var action = () => UserTestFactory.Create(email: email);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenEmailHasExactlyMaximumLength_ShouldCreateUser()
    {
        const string domain = "@example.com";

        var localPart = UserTestFactory.CreateString(
            UserEntity.EmailMaxLength - domain.Length);

        var email = localPart + domain;

        var user = UserTestFactory.Create(email: email);

        user.Email.Should().Be(email);
        user.Email.Should().HaveLength(UserEntity.EmailMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateNew_WhenUsernameMissing_ShouldThrow(
        string? username)
    {
        var action = () => UserTestFactory.Create(username: username!);

        action.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    [InlineData("me")]
    public void CreateNew_WhenUsernameIsShorterThanMinimum_ShouldThrow(
        string username)
    {
        var action = () => UserTestFactory.Create(username: username);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenUsernameExceedsMaximumLength_ShouldThrow()
    {
        var username = UserTestFactory.CreateString(
            UserEntity.UsernameMaxLength + 1);

        var action = () => UserTestFactory.Create(username: username);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenUsernameHasMinimumLength_ShouldCreateUser()
    {
        var username = UserTestFactory.CreateString(
            UserEntity.UsernameMinLength);

        var user = UserTestFactory.Create(username: username);

        user.Username.Should().Be(username);
    }

    [Fact]
    public void CreateNew_WhenUsernameHasMaximumLength_ShouldCreateUser()
    {
        var username = UserTestFactory.CreateString(
            UserEntity.UsernameMaxLength);

        var user = UserTestFactory.Create(username: username);

        user.Username.Should().Be(username);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("ADMIN")]
    [InlineData("Administrator")]
    [InlineData("root")]
    [InlineData("system")]
    [InlineData("support")]
    [InlineData("null")]
    [InlineData("undefined")]
    [InlineData("api")]
    [InlineData("auth")]
    [InlineData("profile")]
    [InlineData("roles")]
    [InlineData("operations")]
    public void CreateNew_WhenUsernameReserved_ShouldThrow(
        string username)
    {
        var action = () => UserTestFactory.Create(username: username);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*username is reserved*");
    }

    [Theory]
    [InlineData("1user")]
    [InlineData("_user")]
    [InlineData(".user")]
    [InlineData("user-name")]
    [InlineData("user name")]
    [InlineData("user@name")]
    [InlineData("user/name")]
    [InlineData("user..name")]
    [InlineData("user.")]
    public void CreateNew_WhenUsernameFormatInvalid_ShouldThrow(
        string username)
    {
        var action = () => UserTestFactory.Create(username: username);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*username format is invalid*");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("test.user")]
    [InlineData("test_user")]
    [InlineData("User123")]
    [InlineData("a.b_c123")]
    public void CreateNew_WhenUsernameFormatValid_ShouldCreateUser(
        string username)
    {
        var user = UserTestFactory.Create(username: username);

        user.Username.Should().Be(username);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateNew_WhenFirstNameMissing_ShouldThrow(
        string? firstName)
    {
        var action = () => UserTestFactory.Create(firstName: firstName!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenFirstNameExceedsMaximumLength_ShouldThrow()
    {
        var firstName = UserTestFactory.CreateString(
            UserEntity.FirstNameMaxLength + 1);

        var action = () => UserTestFactory.Create(firstName: firstName);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenFirstNameHasMaximumLength_ShouldCreateUser()
    {
        var firstName = UserTestFactory.CreateString(
            UserEntity.FirstNameMaxLength);

        var user = UserTestFactory.Create(firstName: firstName);

        user.FirstName.Should().HaveLength(UserEntity.FirstNameMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateNew_WhenLastNameMissing_ShouldThrow(
        string? lastName)
    {
        var action = () => UserTestFactory.Create(lastName: lastName!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenLastNameExceedsMaximumLength_ShouldThrow()
    {
        var lastName = UserTestFactory.CreateString(
            UserEntity.LastNameMaxLength + 1);

        var action = () => UserTestFactory.Create(lastName: lastName);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenLastNameHasMaximumLength_ShouldCreateUser()
    {
        var lastName = UserTestFactory.CreateString(
            UserEntity.LastNameMaxLength);

        var user = UserTestFactory.Create(lastName: lastName);

        user.LastName.Should().HaveLength(UserEntity.LastNameMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateNew_WhenPhoneNumberMissing_ShouldThrow(
        string? phoneNumber)
    {
        var action = () => UserTestFactory.Create(
            phoneNumber: phoneNumber!);

        action.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("9123456789")]
    [InlineData("0912345678")]
    [InlineData("091234567890")]
    [InlineData("+98912345678")]
    [InlineData("+9891234567890")]
    [InlineData("+9809123456789")]
    [InlineData("02112345678")]
    [InlineData("09123abc789")]
    [InlineData("+98912abc678")]
    public void CreateNew_WhenPhoneNumberFormatInvalid_ShouldThrow(
        string phoneNumber)
    {
        var action = () => UserTestFactory.Create(
            phoneNumber: phoneNumber);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*phone number format is invalid*");
    }

    [Theory]
    [InlineData("09123456789")]
    [InlineData("+989123456789")]
    public void CreateNew_WhenPhoneNumberValid_ShouldCreateUser(
        string phoneNumber)
    {
        var user = UserTestFactory.Create(phoneNumber: phoneNumber);

        user.PhoneNumber.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateNew_WhenPasswordHashMissing_ShouldThrow(
        string? passwordHash)
    {
        var action = () => UserTestFactory.Create(
            passwordHash: passwordHash!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenPasswordHashExceedsMaximumLength_ShouldThrow()
    {
        var passwordHash = UserTestFactory.CreateString(
            UserEntity.PasswordHashMaxLength + 1);

        var action = () => UserTestFactory.Create(
            passwordHash: passwordHash);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateNew_WhenPasswordHashHasMaximumLength_ShouldCreateUser()
    {
        var passwordHash = UserTestFactory.CreateString(
            UserEntity.PasswordHashMaxLength);

        var user = UserTestFactory.Create(
            passwordHash: passwordHash);

        user.PasswordHash.Should().HaveLength(
            UserEntity.PasswordHashMaxLength);
    }
}
