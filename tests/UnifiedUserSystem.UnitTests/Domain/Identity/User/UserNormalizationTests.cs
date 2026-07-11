
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UserEntity = UnifiedUserSystem.src.Domain.Identity.Entities.User;


namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserNormalizationTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" test.user ", "test.user")]
    [InlineData("Test.User", "Test.User")]
    public void NormalizeUsername_ShouldReturnExpectedResult(
        string? value,
        string expected)
    {
        UserEntity.NormalizeUsername(value!)
            .Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" USER@EXAMPLE.COM ", "user@example.com")]
    [InlineData("Test.User@Example.Com", "test.user@example.com")]
    public void NormalizeEmail_ShouldReturnExpectedResult(
        string? value,
        string expected)
    {
        UserEntity.NormalizeEmail(value!)
            .Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" Amirhossein ", "Amirhossein")]
    public void NormalizeFirstName_ShouldReturnExpectedResult(
        string? value,
        string expected)
    {
        UserEntity.NormalizeFirstName(value!)
            .Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" Soleimani ", "Soleimani")]
    public void NormalizeLastName_ShouldReturnExpectedResult(
        string? value,
        string expected)
    {
        UserEntity.NormalizeLastName(value!)
            .Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" 09123456789 ", "09123456789")]
    public void NormalizePhoneNumber_ShouldReturnExpectedResult(
        string? value,
        string expected)
    {
        UserEntity.NormalizePhoneNumber(value!)
            .Should()
            .Be(expected);
    }

    [Fact]
    public void CreateNew_ShouldNormalizeSupportedFields()
    {
        var user = UserTestFactory.Create(
            email: " USER@EXAMPLE.COM ",
            username: " test.user ",
            firstName: " Amirhossein ",
            lastName: " Soleimani ",
            phoneNumber: " 09123456789 ");

        user.Email.Should().Be("user@example.com");
        user.Username.Should().Be("test.user");
        user.FirstName.Should().Be("Amirhossein");
        user.LastName.Should().Be("Soleimani");
        user.PhoneNumber.Should().Be("09123456789");
    }

    [Fact]
    public void Fullname_ShouldBeGeneratedFromFirstNameAndLastName()
    {
        var user = UserTestFactory.Create(
            firstName: "Amirhossein",
            lastName: "Soleimani");

        user.Fullname.Should().Be("Amirhossein Soleimani");
    }

    [Theory]
    [InlineData("Amirhossein Soleimani", "Amirhossein", "Soleimani")]
    [InlineData(" Amirhossein Soleimani ", "Amirhossein", "Soleimani")]
    [InlineData("Amirhossein", "Amirhossein", "Amirhossein")]
    [InlineData(
        "Amirhossein Mohammad Soleimani",
        "Amirhossein",
        "Mohammad Soleimani")]
    public void SplitFullName_ShouldReturnExpectedParts(
        string fullname,
        string expectedFirstName,
        string expectedLastName)
    {
        var result = UserEntity.SplitFullName(fullname);

        result.FirstName.Should().Be(expectedFirstName);
        result.LastName.Should().Be(expectedLastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void SplitFullName_WhenValueMissing_ShouldThrow(
        string? fullname)
    {
        var action = () => UserEntity.SplitFullName(fullname!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void SplitFullName_WhenValueExceedsMaximumLength_ShouldThrow()
    {
        var fullname = UserTestFactory.CreateString(
            UserEntity.FullnameMaxLength + 1);

        var action = () => UserEntity.SplitFullName(fullname);

        action.Should().Throw<DomainException>();
    }
}