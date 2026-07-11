using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

using UserEntity =
    UnifiedUserSystem.src.Domain.Identity.Entities.User;


namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserProfileTests
{
    [Fact]
    public void ChangeProfile_WithDifferentValues_ShouldUpdateProfile()
    {
        var user = UserTestFactory.Create();
        var changedAt = UserTestFactory.CreatedAt.AddMinutes(5);
        var actorId = Guid.NewGuid();

        user.ChangeProfile(
            firstName: "Sara",
            lastName: "Ahmadi",
            phoneNumber: "+989123456789",
            nowUtc: changedAt,
            actorUserId: actorId);

        user.FirstName.Should().Be("Sara");
        user.LastName.Should().Be("Ahmadi");
        user.PhoneNumber.Should().Be("+989123456789");
        user.Fullname.Should().Be("Sara Ahmadi");

        user.UpdatedAt.Should().Be(changedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void ChangeProfile_ShouldNormalizeValues()
    {
        var user = UserTestFactory.Create();

        user.ChangeProfile(
            firstName: " Sara ",
            lastName: " Ahmadi ",
            phoneNumber: " +989123456789 ",
            nowUtc: UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        user.FirstName.Should().Be("Sara");
        user.LastName.Should().Be("Ahmadi");
        user.PhoneNumber.Should().Be("+989123456789");
    }

    [Fact]
    public void ChangeProfile_WithoutActor_ShouldUseUserIdForAudit()
    {
        var user = UserTestFactory.Create();
        var changedAt = UserTestFactory.CreatedAt.AddMinutes(1);

        user.ChangeProfile(
            "Sara",
            "Ahmadi",
            "09123456789",
            changedAt,
            actorUserId: null);

        user.UpdatedAt.Should().Be(changedAt);
        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public void ChangeProfile_WhenNormalizedValuesAreUnchanged_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.ChangeProfile(
            firstName: " Amirhossein ",
            lastName: " Soleimani ",
            phoneNumber: " 09123456789 ",
            nowUtc: UserTestFactory.CreatedAt.AddHours(1),
            actorUserId: Guid.NewGuid());

        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData("", "Soleimani", "09123456789")]
    [InlineData(" ", "Soleimani", "09123456789")]
    [InlineData("Amirhossein", "", "09123456789")]
    [InlineData("Amirhossein", " ", "09123456789")]
    [InlineData("Amirhossein", "Soleimani", "")]
    [InlineData("Amirhossein", "Soleimani", "invalid")]
    public void ChangeProfile_WhenInputInvalid_ShouldThrow(
        string firstName,
        string lastName,
        string phoneNumber)
    {
        var user = UserTestFactory.Create();

        var action = () => user.ChangeProfile(
            firstName,
            lastName,
            phoneNumber,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeProfile_WhenValidationFails_ShouldNotPartiallyUpdateUser()
    {
        var user = UserTestFactory.Create();

        var action = () => user.ChangeProfile(
            firstName: "ChangedName",
            lastName: "ChangedLastName",
            phoneNumber: "invalid",
            nowUtc: UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();

        user.FirstName.Should().Be("Amirhossein");
        user.LastName.Should().Be("Soleimani");
        user.PhoneNumber.Should().Be("09123456789");
        user.Fullname.Should().Be("Amirhossein Soleimani");
    }

    [Fact]
    public void ChangeUsername_WithValidDifferentValue_ShouldUpdateUsername()
    {
        var user = UserTestFactory.Create();
        var changedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        user.ChangeUsername(
            "new.user",
            changedAt,
            actorId);

        user.Username.Should().Be("new.user");
        user.UpdatedAt.Should().Be(changedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void ChangeUsername_ShouldNormalizeValue()
    {
        var user = UserTestFactory.Create();

        user.ChangeUsername(
            " new.user ",
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.Username.Should().Be("new.user");
    }

    [Fact]
    public void ChangeUsername_WhenNormalizedValueUnchanged_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.ChangeUsername(
            " test.user ",
            UserTestFactory.CreatedAt.AddMinutes(5),
            Guid.NewGuid());

        user.Username.Should().Be("test.user");
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("1user")]
    [InlineData("user-name")]
    [InlineData("user..name")]
    [InlineData("admin")]
    public void ChangeUsername_WhenValueInvalid_ShouldThrow(
        string username)
    {
        var user = UserTestFactory.Create();

        var action = () => user.ChangeUsername(
            username,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeUsername_WhenValidationFails_ShouldPreserveOriginalUsername()
    {
        var user = UserTestFactory.Create();

        var action = () => user.ChangeUsername(
            "invalid-username",
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        user.Username.Should().Be("test.user");
    }

    [Fact]
    public void ChangePasswordHash_WithDifferentHash_ShouldUpdateHash()
    {
        var user = UserTestFactory.Create();
        var changedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        user.ChangePasswordHash(
            "new-password-hash",
            changedAt,
            actorId);

        user.PasswordHash.Should().Be("new-password-hash");
        user.UpdatedAt.Should().Be(changedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void ChangePasswordHash_WithoutActor_ShouldUseUserId()
    {
        var user = UserTestFactory.Create();
        var changedAt = UserTestFactory.CreatedAt.AddMinutes(1);

        user.ChangePasswordHash(
            "new-password-hash",
            changedAt,
            actorUserId: null);

        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public void ChangePasswordHash_WhenHashUnchanged_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.ChangePasswordHash(
            "valid-password-hash",
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangePasswordHash_WhenValueMissing_ShouldThrow(
        string? passwordHash)
    {
        var user = UserTestFactory.Create();

        var action = () => user.ChangePasswordHash(
            passwordHash!,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangePasswordHash_WhenValueTooLong_ShouldThrow()
    {
        var user = UserTestFactory.Create();

        var tooLongHash = UserTestFactory.CreateString(
            UserEntity
                .PasswordHashMaxLength + 1);

        var action = () => user.ChangePasswordHash(
            tooLongHash,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        user.PasswordHash.Should().Be("valid-password-hash");
    }
}