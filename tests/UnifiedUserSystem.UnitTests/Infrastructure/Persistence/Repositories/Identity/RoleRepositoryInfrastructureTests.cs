using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Identity;

public class RoleRepositoryInfrastructureTests
{
    [Fact]
    public async Task Add_Should_TrackAndPersistRole_When_SaveChangesIsCalled()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RoleRepository(db);
        var role = InfrastructureDbContextFactory.CreateRole("support", "Support");

        repository.Add(role);
        await db.SaveChangesAsync();

        Assert.True(await db.Roles.AnyAsync(x => x.Id == role.Id));
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnRole_When_RoleExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var role = InfrastructureDbContextFactory.CreateRole("support", "Support");
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var repository = new RoleRepository(db);

        var found = await repository.FindByIdAsync(role.Id);

        Assert.NotNull(found);
        Assert.Equal("support", found.Key);
    }

    [Fact]
    public async Task FindByKeyAsync_Should_ReturnRole_When_NormalizedKeyExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        db.Roles.Add(InfrastructureDbContextFactory.CreateRole("support-team", "Support Team"));
        await db.SaveChangesAsync();

        var repository = new RoleRepository(db);

        var found = await repository.FindByKeyAsync("support-team");

        Assert.NotNull(found);
        Assert.Equal("Support Team", found.Name);
    }

    [Fact]
    public async Task ExistsByKeyAsync_Should_ReturnFalse_When_KeyDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RoleRepository(db);

        var exists = await repository.ExistsByKeyAsync("missing");

        Assert.False(exists);
    }

    [Fact]
    public async Task ListAsync_Should_ReturnRolesOrderedByName()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        db.Roles.AddRange(
            InfrastructureDbContextFactory.CreateRole("z", "Z Role"),
            InfrastructureDbContextFactory.CreateRole("a", "A Role"));

        await db.SaveChangesAsync();

        var repository = new RoleRepository(db);

        var roles = await repository.ListAsync();

        Assert.Equal(new[] { "A Role", "Z Role" }, roles.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task HasAssignedUsersAsync_Should_ReturnTrue_When_RoleIsAssignedToUser()
    {
        var now = new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", now);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = InfrastructureDbContextFactory.CreateUser(nowUtc: now);
        user.AssignRole(role.Id, now, user.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new RoleRepository(db);

        var hasUsers = await repository.HasAssignedUsersAsync(role.Id);

        Assert.True(hasUsers);
    }

    [Fact]
    public async Task Delete_Should_SoftDeleteRole_When_RoleIsSafeToDelete()
    {
        // Arrange
        var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var actorUserId = Guid.NewGuid();

        await using var db = InfrastructureDbContextFactory.Create();

        var role = Role.Create("admin", "Admin", now, actorUserId);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var sut = new RoleRepository(db);
        var hasAssignedUsers = await sut.HasAssignedUsersAsync(role.Id);
        hasAssignedUsers.Should().BeFalse();

        // Act
        role.Delete(now.AddMinutes(1), actorUserId);
        await db.SaveChangesAsync();

        // Assert
        var visibleRoles = await db.Roles.ToListAsync();
        visibleRoles.Should().BeEmpty();

        var deletedRole = await db.Roles
            .IgnoreQueryFilters()
            .SingleAsync();

        deletedRole.IsDeleted.Should().BeTrue();
        deletedRole.Name.Should().Be("Admin");
    }
}