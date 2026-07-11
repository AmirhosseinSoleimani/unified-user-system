
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Operations;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Operation")]
public sealed class OperationCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveOperation()
    {
        var operation = OperationTestFactory.Create();

        operation.Id.Should().NotBeEmpty();
        operation.Key.Should().Be("users.view");
        operation.Title.Should().Be("View users");
        operation.IsActive.Should().BeTrue();

        operation.RoleOperations.Should()
            .NotBeNull()
            .And
            .BeEmpty();

        operation.IsDeleted.Should().BeFalse();
        operation.DeletedAt.Should().BeNull();
        operation.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithActor_ShouldInitializeAuditFields()
    {
        var operation = OperationTestFactory.Create(
            actorUserId: OperationTestFactory.ActorUserId);

        operation.CreatedAt.Should()
            .Be(OperationTestFactory.CreatedAt);

        operation.UpdatedAt.Should()
            .Be(OperationTestFactory.CreatedAt);

        operation.CreatedByUserId.Should()
            .Be(OperationTestFactory.ActorUserId);

        operation.UpdatedByUserId.Should()
            .Be(OperationTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldKeepAuditActorNull()
    {
        var operation = OperationTestFactory.Create(
            actorUserId: null);

        operation.CreatedByUserId.Should().BeNull();
        operation.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldGenerateDifferentIds()
    {
        var firstOperation = OperationTestFactory.Create(
            key: "users.view");

        var secondOperation = OperationTestFactory.Create(
            key: "users.create");

        firstOperation.Id.Should()
            .NotBe(secondOperation.Id);
    }

    [Theory]
    [InlineData(" USERS.VIEW ", "users.view")]
    [InlineData("Users.Create", "users.create")]
    [InlineData("ROLE-ASSIGN", "role-assign")]
    [InlineData("USER_UPDATE", "user_update")]
    public void Create_ShouldNormalizeKey(
        string input,
        string expected)
    {
        var operation = OperationTestFactory.Create(
            key: input);

        operation.Key.Should().Be(expected);
    }

    [Theory]
    [InlineData(" View users ", "View users")]
    [InlineData("Create user", "Create user")]
    public void Create_ShouldNormalizeTitle(
        string input,
        string expected)
    {
        var operation = OperationTestFactory.Create(
            title: input);

        operation.Title.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenKeyMissing_ShouldThrow(
        string? key)
    {
        var action = () => OperationTestFactory.Create(
            key: key!);

        action.Should()
            .Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenKeyExceedsMaximumLength_ShouldThrow()
    {
        var key = OperationTestFactory.CreateString(
            Operation.KeyMaxLength + 1);

        var action = () => OperationTestFactory.Create(
            key: key);

        action.Should()
            .Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenKeyHasMaximumLength_ShouldCreateOperation()
    {
        var key = OperationTestFactory.CreateString(
            Operation.KeyMaxLength);

        var operation = OperationTestFactory.Create(
            key: key);

        operation.Key.Should()
            .HaveLength(Operation.KeyMaxLength);
    }

    [Theory]
    [InlineData("users@view")]
    [InlineData("users/view")]
    [InlineData("users view")]
    [InlineData("users:view")]
    [InlineData("کاربران.view")]
    [InlineData("users#view")]
    [InlineData("users+view")]
    public void Create_WhenKeyContainsInvalidCharacter_ShouldThrow(
        string key)
    {
        var action = () => OperationTestFactory.Create(
            key: key);

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Operation key contains invalid characters*");
    }

    [Theory]
    [InlineData("users.view")]
    [InlineData("users-create")]
    [InlineData("users_update")]
    [InlineData("user1.view2")]
    [InlineData("a")]
    [InlineData("1")]
    [InlineData(".")]
    [InlineData("-")]
    [InlineData("_")]
    public void Create_WhenKeyCharactersAreAllowed_ShouldCreateOperation(
        string key)
    {
        var operation = OperationTestFactory.Create(
            key: key);

        operation.Key.Should().Be(key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenTitleMissing_ShouldThrow(
        string? title)
    {
        var action = () => OperationTestFactory.Create(
            title: title!);

        action.Should()
            .Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenTitleExceedsMaximumLength_ShouldThrow()
    {
        var title = OperationTestFactory.CreateString(
            Operation.TitleMaxLength + 1);

        var action = () => OperationTestFactory.Create(
            title: title);

        action.Should()
            .Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenTitleHasMaximumLength_ShouldCreateOperation()
    {
        var title = OperationTestFactory.CreateString(
            Operation.TitleMaxLength);

        var operation = OperationTestFactory.Create(
            title: title);

        operation.Title.Should()
            .HaveLength(Operation.TitleMaxLength);
    }
}
