
using FluentAssertions;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Operations;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Operation")]
public sealed class OperationSoftDeleteTests
{
    [Fact]
    public void Delete_ShouldSoftDeleteOperation()
    {
        var operation = OperationTestFactory.Create();

        var deletedAt =
            OperationTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        operation.Delete(
            deletedAt,
            actorId);

        operation.IsDeleted.Should().BeTrue();
        operation.DeletedAt.Should().Be(deletedAt);
        operation.DeletedByUserId.Should().Be(actorId);

        operation.UpdatedAt.Should().Be(deletedAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        var firstDeletedAt =
            OperationTestFactory.CreatedAt.AddMinutes(1);

        var firstActor = Guid.NewGuid();

        operation.Delete(
            firstDeletedAt,
            firstActor);

        operation.Delete(
            OperationTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        operation.DeletedAt.Should().Be(firstDeletedAt);
        operation.DeletedByUserId.Should().Be(firstActor);
        operation.UpdatedAt.Should().Be(firstDeletedAt);
        operation.UpdatedByUserId.Should().Be(firstActor);
    }

    [Fact]
    public void UnDelete_WhenDeleted_ShouldRestoreOperation()
    {
        var operation = OperationTestFactory.Create();

        operation.Delete(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var restoredAt =
            OperationTestFactory.CreatedAt.AddMinutes(2);

        var actorId = Guid.NewGuid();

        operation.UnDelete(
            restoredAt,
            actorId);

        operation.IsDeleted.Should().BeFalse();
        operation.DeletedAt.Should().BeNull();
        operation.DeletedByUserId.Should().BeNull();

        operation.UpdatedAt.Should().Be(restoredAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void UnDelete_WhenNotDeleted_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        operation.UnDelete(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.IsDeleted.Should().BeFalse();
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void DeleteAndRestore_ShouldPreserveOperationData()
    {
        var operation = OperationTestFactory.Create();

        operation.Delete(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.UnDelete(
            OperationTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        operation.Key.Should().Be("users.view");
        operation.Title.Should().Be("View users");
        operation.IsActive.Should().BeTrue();
    }
}
