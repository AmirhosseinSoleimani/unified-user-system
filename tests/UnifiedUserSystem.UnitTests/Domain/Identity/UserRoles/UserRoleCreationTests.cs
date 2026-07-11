
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.UserRoles;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "UserRole")]
public sealed class UserRoleCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUserRole()
    {
        var userRole = UserRoleTestFactory.Create();

        userRole.Id.Should().NotBeEmpty();
        userRole.UserId.Should().Be(UserRoleTestFactory.UserId);
        userRole.RoleId.Should().Be(UserRoleTestFactory.RoleId);

        userRole.IsDeleted.Should().BeFalse();
        userRole.DeletedAt.Should().BeNull();
        userRole.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithActor_ShouldInitializeAuditFields()
    {
        var userRole = UserRoleTestFactory.Create(
            actorUserId: UserRoleTestFactory.ActorUserId);

        userRole.CreatedAt.Should()
            .Be(UserRoleTestFactory.CreatedAt);

        userRole.UpdatedAt.Should()
            .Be(UserRoleTestFactory.CreatedAt);

        userRole.CreatedByUserId.Should()
            .Be(UserRoleTestFactory.ActorUserId);

        userRole.UpdatedByUserId.Should()
            .Be(UserRoleTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldUseUserIdForAudit()
    {
        var userRole = UserRoleTestFactory.Create(
            actorUserId: null);

        userRole.CreatedByUserId.Should()
            .Be(UserRoleTestFactory.UserId);

        userRole.UpdatedByUserId.Should()
            .Be(UserRoleTestFactory.UserId);
    }

    [Fact]
    public void Create_ShouldGenerateDifferentIds()
    {
        var first = UserRoleTestFactory.Create();

        var second = UserRoleTestFactory.Create(
            roleId: 20);

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Create_WhenUserIdIsEmpty_ShouldThrow()
    {
        var action = () => UserRoleTestFactory.Create(
            userId: Guid.Empty);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*UserId is invalid*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(int.MinValue)]
    public void Create_WhenRoleIdIsNotPositive_ShouldThrow(
        int roleId)
    {
        var action = () => UserRoleTestFactory.Create(
            roleId: roleId);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*RoleId is invalid*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Create_WhenRoleIdIsPositive_ShouldCreateUserRole(
        int roleId)
    {
        var userRole = UserRoleTestFactory.Create(
            roleId: roleId);

        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void Create_WhenValidationFails_ShouldNotReturnEntity()
    {
        var action = () => UserRoleTestFactory.Create(
            userId: Guid.Empty,
            roleId: 10);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithDifferentUsersAndSameRole_ShouldGenerateDifferentEntities()
    {
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        var first = UserRoleTestFactory.Create(
            userId: firstUserId,
            roleId: 10);

        var second = UserRoleTestFactory.Create(
            userId: secondUserId,
            roleId: 10);

        first.Id.Should().NotBe(second.Id);

        first.UserId.Should().Be(firstUserId);
        second.UserId.Should().Be(secondUserId);

        first.RoleId.Should().Be(second.RoleId);
    }

    [Fact]
    public void Create_WithSameUserAndDifferentRoles_ShouldGenerateDifferentEntities()
    {
        var first = UserRoleTestFactory.Create(
            roleId: 10);

        var second = UserRoleTestFactory.Create(
            roleId: 20);

        first.Id.Should().NotBe(second.Id);

        first.UserId.Should().Be(second.UserId);
        first.RoleId.Should().NotBe(second.RoleId);
    }
}
