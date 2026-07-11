using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserSoftDeleteTests
{
    [Fact]
    public void NewUser_ShouldNotBeDeleted()
    {
        var user = UserTestFactory.Create();

        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletionFields()
    {
        var user = UserTestFactory.Create();

        var deletedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        user.SoftDelete(deletedAt, actorId);

        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().Be(deletedAt);
        user.DeletedByUserId.Should().Be(actorId);

        user.UpdatedAt.Should().Be(deletedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void SoftDelete_WithNullActor_ShouldPreserveNullActor()
    {
        var user = UserTestFactory.Create();

        user.SoftDelete(
            UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        user.IsDeleted.Should().BeTrue();
        user.DeletedByUserId.Should().BeNull();
        user.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var firstDeletedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var firstActorId = Guid.NewGuid();

        user.SoftDelete(firstDeletedAt, firstActorId);

        user.SoftDelete(
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().Be(firstDeletedAt);
        user.DeletedByUserId.Should().Be(firstActorId);
        user.UpdatedAt.Should().Be(firstDeletedAt);
        user.UpdatedByUserId.Should().Be(firstActorId);
    }

    [Fact]
    public void Restore_WhenDeleted_ShouldClearDeletionFields()
    {
        var user = UserTestFactory.Create();

        user.SoftDelete(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var restoredAt = UserTestFactory.CreatedAt.AddMinutes(2);
        var actorId = Guid.NewGuid();

        user.Restore(restoredAt, actorId);

        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.DeletedByUserId.Should().BeNull();

        user.UpdatedAt.Should().Be(restoredAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Restore_WithNullActor_ShouldPreserveNullAuditActor()
    {
        var user = UserTestFactory.Create();

        user.SoftDelete(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.Restore(
            UserTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: null);

        user.IsDeleted.Should().BeFalse();
        user.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenUserNotDeleted_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.Restore(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.DeletedByUserId.Should().BeNull();

        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void SoftDeleteThenRestore_ShouldPreserveUserData()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.SoftDelete(
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.Restore(
            UserTestFactory.CreatedAt.AddMinutes(3),
            Guid.NewGuid());

        user.Email.Should().Be("user@example.com");
        user.Username.Should().Be("test.user");
        user.FirstName.Should().Be("Amirhossein");
        user.LastName.Should().Be("Soleimani");
        user.PhoneNumber.Should().Be("09123456789");
        user.PasswordHash.Should().Be("valid-password-hash");
        user.UserRoles.Should().ContainSingle();
    }
}
