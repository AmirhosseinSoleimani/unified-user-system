using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Roles;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Role")]
public sealed class RoleCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveRole()
    {
        var role = RoleTestFactory.Create();

        role.Key.Should().Be("system-admin");
        role.Name.Should().Be("System Administrator");
        role.IsActive.Should().BeTrue();

        role.UserRoles.Should().NotBeNull().And.BeEmpty();
        role.RoleOperations.Should().NotBeNull().And.BeEmpty();

        role.IsDeleted.Should().BeFalse();
        role.DeletedAt.Should().BeNull();
        role.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithActor_ShouldInitializeAuditFields()
    {
        var role = RoleTestFactory.Create(
            actorUserId: RoleTestFactory.ActorUserId);

        role.CreatedAt.Should().Be(RoleTestFactory.CreatedAt);
        role.UpdatedAt.Should().Be(RoleTestFactory.CreatedAt);

        role.CreatedByUserId.Should()
            .Be(RoleTestFactory.ActorUserId);

        role.UpdatedByUserId.Should()
            .Be(RoleTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldKeepAuditActorNull()
    {
        var role = RoleTestFactory.Create(actorUserId: null);

        role.CreatedByUserId.Should().BeNull();
        role.UpdatedByUserId.Should().BeNull();
    }

    [Theory]
    [InlineData(" Admin ", "admin")]
    [InlineData("SYSTEM ADMIN", "system-admin")]
    [InlineData("  system   admin  ", "system-admin")]
    [InlineData("Support Manager", "support-manager")]
    public void Create_ShouldNormalizeKey(
        string input,
        string expected)
    {
        var role = RoleTestFactory.Create(key: input);

        role.Key.Should().Be(expected);
    }

    [Theory]
    [InlineData(" Admin ", "Admin")]
    [InlineData(" System Administrator ", "System Administrator")]
    [InlineData("Support Manager", "Support Manager")]
    public void Create_ShouldNormalizeName(
        string input,
        string expected)
    {
        var role = RoleTestFactory.Create(name: input);

        role.Name.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenKeyMissing_ShouldThrow(
        string? key)
    {
        var action = () => RoleTestFactory.Create(key: key!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenKeyExceedsMaximumLength_ShouldThrow()
    {
        var key = RoleTestFactory.CreateString(
            Role.KeyMaxLength + 1);

        var action = () => RoleTestFactory.Create(key: key);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenKeyHasMaximumLength_ShouldCreateRole()
    {
        var key = RoleTestFactory.CreateString(
            Role.KeyMaxLength);

        var role = RoleTestFactory.Create(key: key);

        role.Key.Should().HaveLength(Role.KeyMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenNameMissing_ShouldThrow(
        string? name)
    {
        var action = () => RoleTestFactory.Create(name: name!);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenNameExceedsMaximumLength_ShouldThrow()
    {
        var name = RoleTestFactory.CreateString(
            Role.NameMaxLength + 1);

        var action = () => RoleTestFactory.Create(name: name);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenNameHasMaximumLength_ShouldCreateRole()
    {
        var name = RoleTestFactory.CreateString(
            Role.NameMaxLength);

        var role = RoleTestFactory.Create(name: name);

        role.Name.Should().HaveLength(Role.NameMaxLength);
    }
}