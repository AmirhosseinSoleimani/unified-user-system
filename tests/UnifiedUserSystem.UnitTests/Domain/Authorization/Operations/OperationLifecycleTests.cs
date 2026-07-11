
using FluentAssertions;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Operations;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Operation")]
public sealed class OperationLifecycleTests
{
    [Fact]
    public void Create_ShouldCreateActiveOperation()
    {
        var operation = OperationTestFactory.Create();

        operation.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateOperation()
    {
        var operation = OperationTestFactory.Create();

        var deactivatedAt =
            OperationTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        operation.Deactivate(
            deactivatedAt,
            actorId);

        operation.IsActive.Should().BeFalse();
        operation.UpdatedAt.Should().Be(deactivatedAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        operation.Deactivate(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            OperationTestFactory.ActorUserId);

        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        operation.Deactivate(
            OperationTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        operation.IsActive.Should().BeFalse();
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateOperation()
    {
        var operation = OperationTestFactory.Create();

        operation.Deactivate(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var activatedAt =
            OperationTestFactory.CreatedAt.AddMinutes(2);

        var actorId = Guid.NewGuid();

        operation.Activate(
            activatedAt,
            actorId);

        operation.IsActive.Should().BeTrue();
        operation.UpdatedAt.Should().Be(activatedAt);
        operation.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldBeNoOp()
    {
        var operation = OperationTestFactory.Create();

        var originalUpdatedAt = operation.UpdatedAt;
        var originalUpdatedBy = operation.UpdatedByUserId;

        operation.Activate(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.IsActive.Should().BeTrue();
        operation.UpdatedAt.Should().Be(originalUpdatedAt);
        operation.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void DeactivateThenActivate_ShouldPreserveOperationData()
    {
        var operation = OperationTestFactory.Create();

        operation.Deactivate(
            OperationTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        operation.Activate(
            OperationTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        operation.Key.Should().Be("users.view");
        operation.Title.Should().Be("View users");
    }
}
