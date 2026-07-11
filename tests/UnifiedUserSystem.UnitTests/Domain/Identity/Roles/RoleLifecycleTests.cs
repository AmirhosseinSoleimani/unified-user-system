
using FluentAssertions;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Roles;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Role")]
public sealed class RoleLifecycleTests
{
    [Fact]
    public void Create_ShouldCreateActiveRole()
    {
        var role = RoleTestFactory.Create();

        role.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateRole()
    {
        var role = RoleTestFactory.Create();

        var deactivatedAt =
            RoleTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        role.Deactivate(deactivatedAt, actorId);

        role.IsActive.Should().BeFalse();
        role.UpdatedAt.Should().Be(deactivatedAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        role.Deactivate(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            RoleTestFactory.ActorUserId);

        var originalUpdatedAt = role.UpdatedAt;
        var originalUpdatedBy = role.UpdatedByUserId;

        role.Deactivate(
            RoleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        role.IsActive.Should().BeFalse();
        role.UpdatedAt.Should().Be(originalUpdatedAt);
        role.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateRole()
    {
        var role = RoleTestFactory.Create();

        role.Deactivate(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var activatedAt =
            RoleTestFactory.CreatedAt.AddMinutes(2);

        var actorId = Guid.NewGuid();

        role.Activate(activatedAt, actorId);

        role.IsActive.Should().BeTrue();
        role.UpdatedAt.Should().Be(activatedAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        var originalUpdatedAt = role.UpdatedAt;
        var originalUpdatedBy = role.UpdatedByUserId;

        role.Activate(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.IsActive.Should().BeTrue();
        role.UpdatedAt.Should().Be(originalUpdatedAt);
        role.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void DeactivateThenActivate_ShouldPreserveRoleData()
    {
        var role = RoleTestFactory.Create();

        role.Deactivate(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.Activate(
            RoleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        role.Key.Should().Be("system-admin");
        role.Name.Should().Be("System Administrator");
    }
}
