using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserRoleTests
{
    [Fact]
    public void AssignRole_WithValidRoleId_ShouldAddRole()
    {
        var user = UserTestFactory.Create();
        var assignedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        user.AssignRole(
            roleId: 10,
            nowUtc: assignedAt,
            actorUserId: actorId);

        user.UserRoles.Should().ContainSingle();

        var userRole = user.UserRoles.Single();

        userRole.Id.Should().NotBeEmpty();
        userRole.UserId.Should().Be(user.Id);
        userRole.RoleId.Should().Be(10);

        userRole.CreatedAt.Should().Be(assignedAt);
        userRole.CreatedByUserId.Should().Be(actorId);

        user.UpdatedAt.Should().Be(assignedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void AssignRole_WithoutActor_ShouldUseUserIdForAudits()
    {
        var user = UserTestFactory.Create();
        var assignedAt = UserTestFactory.CreatedAt.AddMinutes(1);

        user.AssignRole(
            roleId: 10,
            nowUtc: assignedAt,
            actorUserId: null);

        var userRole = user.UserRoles.Single();

        userRole.CreatedByUserId.Should().Be(user.Id);
        userRole.UpdatedByUserId.Should().Be(user.Id);
        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AssignRole_WhenRoleIdInvalid_ShouldThrow(
        int roleId)
    {
        var user = UserTestFactory.Create();

        var action = () => user.AssignRole(
            roleId,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*RoleId is invalid*");
    }

    [Fact]
    public void AssignRole_WhenRoleIdInvalid_ShouldNotChangeUser()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        var action = () => user.AssignRole(
            0,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        user.UserRoles.Should().BeEmpty();
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            roleId: 10,
            nowUtc: UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: UserTestFactory.ActorUserId);

        var originalRole = user.UserRoles.Single();
        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.AssignRole(
            roleId: 10,
            nowUtc: UserTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: Guid.NewGuid());

        user.UserRoles.Should().ContainSingle();
        user.UserRoles.Single().Should().BeSameAs(originalRole);

        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void AssignRole_WithDifferentRoleIds_ShouldAddMultipleRoles()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.AssignRole(
            20,
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.UserRoles.Should().HaveCount(2);

        user.UserRoles
            .Select(userRole => userRole.RoleId)
            .Should()
            .BeEquivalentTo([10, 20]);
    }

    [Fact]
    public void RemoveRole_WhenRoleExists_ShouldRemoveRole()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        var removedAt = UserTestFactory.CreatedAt.AddMinutes(2);
        var actorId = Guid.NewGuid();

        user.RemoveRole(10, removedAt, actorId);

        user.UserRoles.Should().BeEmpty();
        user.UpdatedAt.Should().Be(removedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void RemoveRole_WithoutActor_ShouldUseUserIdForAudit()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.RemoveRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: null);

        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void RemoveRole_WhenRoleIdInvalid_ShouldThrow(
        int roleId)
    {
        var user = UserTestFactory.Create();

        var action = () => user.RemoveRole(
            roleId,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*RoleId is invalid*");
    }

    [Fact]
    public void RemoveRole_WhenRoleDoesNotExist_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.RemoveRole(
            roleId: 999,
            nowUtc: UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        user.UserRoles.Should().BeEmpty();
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveOnlyRequestedRole()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.AssignRole(
            20,
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.RemoveRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(3),
            Guid.NewGuid());

        user.UserRoles.Should().ContainSingle();
        user.UserRoles.Single().RoleId.Should().Be(20);
    }

    [Fact]
    public void RemovedRole_CanBeAssignedAgain()
    {
        var user = UserTestFactory.Create();

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.RemoveRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.AssignRole(
            10,
            UserTestFactory.CreatedAt.AddMinutes(3),
            Guid.NewGuid());

        user.UserRoles.Should().ContainSingle();
        user.UserRoles.Single().RoleId.Should().Be(10);
    }
}
