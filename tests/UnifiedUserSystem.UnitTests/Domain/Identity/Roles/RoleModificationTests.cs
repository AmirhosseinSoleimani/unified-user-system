
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Roles;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "Role")]
public sealed class RoleModificationTests
{
    [Fact]
    public void Rename_WithDifferentName_ShouldUpdateName()
    {
        var role = RoleTestFactory.Create();

        var updatedAt = RoleTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        role.Rename(
            newName: "Application Administrator",
            nowUtc: updatedAt,
            actorUserId: actorId);

        role.Name.Should().Be("Application Administrator");
        role.UpdatedAt.Should().Be(updatedAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Rename_ShouldNormalizeName()
    {
        var role = RoleTestFactory.Create();

        role.Rename(
            " Application Administrator ",
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.Name.Should().Be("Application Administrator");
    }

    [Fact]
    public void Rename_WhenNormalizedNameUnchanged_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        var originalUpdatedAt = role.UpdatedAt;
        var originalUpdatedBy = role.UpdatedByUserId;

        role.Rename(
            " System Administrator ",
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.Name.Should().Be("System Administrator");
        role.UpdatedAt.Should().Be(originalUpdatedAt);
        role.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Rename_WhenNameMissing_ShouldThrow(
        string? name)
    {
        var role = RoleTestFactory.Create();

        var action = () => role.Rename(
            name!,
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_WhenNameTooLong_ShouldPreserveOriginalName()
    {
        var role = RoleTestFactory.Create();

        var tooLongName = RoleTestFactory.CreateString(
            Role.NameMaxLength + 1);

        var action = () => role.Rename(
            tooLongName,
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        role.Name.Should().Be("System Administrator");
    }

    [Fact]
    public void ChangeKey_WithDifferentKey_ShouldUpdateKey()
    {
        var role = RoleTestFactory.Create();

        var updatedAt = RoleTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        role.ChangeKey(
            newKey: "application-admin",
            nowUtc: updatedAt,
            actorUserId: actorId);

        role.Key.Should().Be("application-admin");
        role.UpdatedAt.Should().Be(updatedAt);
        role.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void ChangeKey_ShouldNormalizeKey()
    {
        var role = RoleTestFactory.Create();

        role.ChangeKey(
            " APPLICATION ADMIN ",
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.Key.Should().Be("application-admin");
    }

    [Fact]
    public void ChangeKey_WhenNormalizedKeyUnchanged_ShouldBeNoOp()
    {
        var role = RoleTestFactory.Create();

        var originalUpdatedAt = role.UpdatedAt;
        var originalUpdatedBy = role.UpdatedByUserId;

        role.ChangeKey(
            " SYSTEM ADMIN ",
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        role.Key.Should().Be("system-admin");
        role.UpdatedAt.Should().Be(originalUpdatedAt);
        role.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeKey_WhenKeyMissing_ShouldThrow(
        string? key)
    {
        var role = RoleTestFactory.Create();

        var action = () => role.ChangeKey(
            key!,
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeKey_WhenKeyTooLong_ShouldPreserveOriginalKey()
    {
        var role = RoleTestFactory.Create();

        var tooLongKey = RoleTestFactory.CreateString(
            Role.KeyMaxLength + 1);

        var action = () => role.ChangeKey(
            tooLongKey,
            RoleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        role.Key.Should().Be("system-admin");
    }
}
