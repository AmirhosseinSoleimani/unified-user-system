using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Roles;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Role")]
public sealed class RoleSoftDeleteTests
{
    [Fact]
    public void Delete_ShouldSoftDeleteRole()
    {
        var role = RoleTestFactory.Create();

        var deletedAt =
            RoleTestFactory.CreatedAt.AddMinutes(1);

        var actorId = Guid.NewGuid();

        role.Delete(deletedAt, actorId);

        role.IsDeleted.Should().BeTrue();
        role.DeletedAt.Should().Be(deletedAt);
        role.DeletedByUserId.Should().Be(actorId);

        role.UpdatedAt.Should().Be(deletedAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        var firstDeletedAt =
            RoleTestFactory.CreatedAt.AddMinutes(1);

        var firstActor = Guid.NewGuid();

        role.Delete(firstDeletedAt, firstActor);

        role.Delete(
            RoleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        role.DeletedAt.Should().Be(firstDeletedAt);
        role.DeletedByUserId.Should().Be(firstActor);
        role.UpdatedAt.Should().Be(firstDeletedAt);
        role.UpdatedByUserId.Should().Be(firstActor);
    }

    [Fact]
    public void UnDelete_WhenDeleted_ShouldRestoreRole()
    {
        var role = RoleTestFactory.Create();

        role.Delete(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var restoredAt =
            RoleTestFactory.CreatedAt.AddMinutes(2);

        var actorId = Guid.NewGuid();

        role.UnDelete(restoredAt, actorId);

        role.IsDeleted.Should().BeFalse();
        role.DeletedAt.Should().BeNull();
        role.DeletedByUserId.Should().BeNull();

        role.UpdatedAt.Should().Be(restoredAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void UnDelete_WhenNotDeleted_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        var originalUpdatedAt = role.UpdatedAt;
        var originalUpdatedBy = role.UpdatedByUserId;

        role.UnDelete(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.IsDeleted.Should().BeFalse();
        role.UpdatedAt.Should().Be(originalUpdatedAt);
        role.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void DeleteAndRestore_ShouldPreserveRoleData()
    {
        var role = RoleTestFactory.Create();

        role.Delete(
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.UnDelete(
            RoleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        role.Key.Should().Be("system-admin");
        role.Name.Should().Be("System Administrator");
        role.IsActive.Should().BeTrue();
    }
}
