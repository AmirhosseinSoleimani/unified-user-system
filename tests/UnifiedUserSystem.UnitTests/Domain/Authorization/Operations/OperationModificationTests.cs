
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Operations;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Operation")]
public sealed class OperationModificationTests
{
    [Fact]
    public void RenameTitle_WithDifferentTitle_ShouldUpdateTitle()
    {
        var operation = OperationTestFactory.Create();

        var updatedAt =
            OperationTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        operation.RenameTitle(
            newTitle: "View all users",
            nowUtc: updatedAt,
            actorUserId: actorId);

        operation.Title.Should().Be("View all users");
        operation.UpdatedAt.Should().Be(updatedAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void RenameTitle_ShouldNormalizeTitle()
    {
        var operation = OperationTestFactory.Create();

        operation.RenameTitle(
            " View all users ",
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.Title.Should().Be("View all users");
    }

    [Fact]
    public void RenameTitle_WhenNormalizedTitleUnchanged_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        operation.RenameTitle(
            " View users ",
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.Title.Should().Be("View users");
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RenameTitle_WhenTitleMissing_ShouldThrow(
        string? title)
    {
        var operation = OperationTestFactory.Create();

        var action = () => operation.RenameTitle(
            title!,
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>();
    }

    [Fact]
    public void RenameTitle_WhenTitleTooLong_ShouldPreserveOriginalTitle()
    {
        var operation = OperationTestFactory.Create();

        var tooLongTitle = OperationTestFactory.CreateString(
            Operation.TitleMaxLength + 1);

        var originalUpdatedAt = operation.UpdatedAt;

        var action = () => operation.RenameTitle(
            tooLongTitle,
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>();

        operation.Title.Should().Be("View users");
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void ChangeKey_WithDifferentKey_ShouldUpdateKey()
    {
        var operation = OperationTestFactory.Create();

        var updatedAt =
            OperationTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        operation.ChangeKey(
            newKey: "users.list",
            nowUtc: updatedAt,
            actorUserId: actorId);

        operation.Key.Should().Be("users.list");
        operation.UpdatedAt.Should().Be(updatedAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void ChangeKey_ShouldNormalizeKey()
    {
        var operation = OperationTestFactory.Create();

        operation.ChangeKey(
            " USERS.LIST ",
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.Key.Should().Be("users.list");
    }

    [Fact]
    public void ChangeKey_WhenNormalizedKeyUnchanged_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        operation.ChangeKey(
            " USERS.VIEW ",
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.Key.Should().Be("users.view");
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeKey_WhenKeyMissing_ShouldThrow(
        string? key)
    {
        var operation = OperationTestFactory.Create();

        var action = () => operation.ChangeKey(
            key!,
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>();
    }

    [Theory]
    [InlineData("users@list")]
    [InlineData("users/list")]
    [InlineData("users list")]
    [InlineData("users:list")]
    public void ChangeKey_WhenKeyContainsInvalidCharacter_ShouldThrow(
        string key)
    {
        var operation = OperationTestFactory.Create();

        var action = () => operation.ChangeKey(
            key,
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Operation key contains invalid characters*");
    }

    [Fact]
    public void ChangeKey_WhenValidationFails_ShouldPreserveOriginalState()
    {
        var operation = OperationTestFactory.Create();

        var originalKey = operation.Key;
        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        var action = () => operation.ChangeKey(
            "invalid@key",
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<ArgumentException>();

        operation.Key.Should().Be(originalKey);
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void ChangeKey_WhenKeyTooLong_ShouldPreserveOriginalState()
    {
        var operation = OperationTestFactory.Create();

        var tooLongKey = OperationTestFactory.CreateString(
            Operation.KeyMaxLength + 1);

        var originalKey = operation.Key;
        var originalUpdatedAt = operation.UpdatedAt;

        var action = () => operation.ChangeKey(
            tooLongKey,
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>();

        operation.Key.Should().Be(originalKey);
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
    }
}
