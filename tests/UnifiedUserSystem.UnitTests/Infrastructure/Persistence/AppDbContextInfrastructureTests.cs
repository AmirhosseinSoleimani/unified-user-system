using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence;

public class AppDbContextInfrastructureTests
{
    private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);

    [Fact]
    public async Task SaveChangesAsync_Should_SetAuditFields_When_AuditableEntityIsAdded()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var role = InfrastructureDbContextFactory.CreateRole("support", "Support", T1, actor);

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        role.CreatedAt.Should().Be(T1);
        role.UpdatedAt.Should().Be(T1);
        role.CreatedByUserId.Should().Be(actor);
        role.UpdatedByUserId.Should().Be(actor);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_TouchUpdatedFieldsOnly_When_AuditableEntityIsModified()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var role = InfrastructureDbContextFactory.CreateRole("support", "Support", T1, actor);

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var createdAt = role.CreatedAt;
        var createdBy = role.CreatedByUserId;

        clock.Utcnow = T2;
        role.Rename("Support Team", T2, actor);
        await db.SaveChangesAsync();

        role.CreatedAt.Should().Be(createdAt);
        role.CreatedByUserId.Should().Be(createdBy);
        role.UpdatedAt.Should().Be(T2);
        role.UpdatedByUserId.Should().Be(actor);
    }

    [Fact]
    public void SaveChanges_Should_ApplyAudit_When_SynchronousSaveChangesIsUsed()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1, actor);

        db.Roles.Add(role);
        db.SaveChanges();

        role.CreatedAt.Should().Be(T1);
        role.UpdatedAt.Should().Be(T1);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_SoftDeleteUserInsteadOfPhysicallyDeleting_When_DbSetRemoveIsUsed()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var user = InfrastructureDbContextFactory.CreateUser("delete@example.com", "deleteuser", nowUtc: T1, actorUserId: actor);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        db.Users.Should().BeEmpty();

        var deleted = await db.Users.IgnoreQueryFilters().SingleAsync();
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().Be(T2);
        deleted.DeletedByUserId.Should().Be(actor);
        deleted.UpdatedAt.Should().Be(T2);
        deleted.UpdatedByUserId.Should().Be(actor);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_SoftDeleteRoleInsteadOfPhysicallyDeleting_When_DbSetRemoveIsUsed()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1, actor);

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        db.Roles.Remove(role);
        await db.SaveChangesAsync();

        db.Roles.Should().BeEmpty();

        var deleted = await db.Roles.IgnoreQueryFilters().SingleAsync();
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().Be(T2);
        deleted.DeletedByUserId.Should().Be(actor);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_SoftDeleteOperationInsteadOfPhysicallyDeleting_When_DbSetRemoveIsUsed()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1, actor);

        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        db.Operation.Remove(operation);
        await db.SaveChangesAsync();

        db.Operation.Should().BeEmpty();

        var deleted = await db.Operation.IgnoreQueryFilters().SingleAsync();
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().Be(T2);
        deleted.DeletedByUserId.Should().Be(actor);
    }

    [Fact]
    public async Task QueryFilters_Should_ExcludeSoftDeletedUsersRolesAndOperations_When_QueryingNormally()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };
        var databaseName = Guid.NewGuid().ToString();

        await using (var db = InfrastructureDbContextFactory.Create(clock, actor, databaseName: databaseName))
        {
            var activeUser = InfrastructureDbContextFactory.CreateUser("active@example.com", "activeuser", nowUtc: T1, actorUserId: actor);
            var deletedUser = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1, actorUserId: actor);
            deletedUser.SoftDelete(T2, actor);

            var activeRole = InfrastructureDbContextFactory.CreateRole("active", "Active", T1, actor);
            var deletedRole = InfrastructureDbContextFactory.CreateRole("deleted", "Deleted", T1, actor);
            deletedRole.Delete(T2, actor);

            var activeOperation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1, actor);
            var deletedOperation = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1, actor);
            deletedOperation.Delete(T2, actor);

            db.Users.AddRange(activeUser, deletedUser);
            db.Roles.AddRange(activeRole, deletedRole);
            db.Operation.AddRange(activeOperation, deletedOperation);

            await db.SaveChangesAsync();
        }

        await using (var db = InfrastructureDbContextFactory.Create(clock, actor, databaseName: databaseName))
        {
            var users = await db.Users.ToListAsync();
            var roles = await db.Roles.ToListAsync();
            var operations = await db.Operation.ToListAsync();

            users.Select(x => x.Email).Should().Equal("active@example.com");
            roles.Select(x => x.Key).Should().Equal("active");
            operations.Select(x => x.Key).Should().Equal("users.read");
        }
    }

    [Fact]
    public async Task IgnoreQueryFilters_Should_ReturnSoftDeletedEntities_When_ExplicitlyRequested()
    {
        var actor = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock, actor);

        var deletedUser = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1, actorUserId: actor);
        deletedUser.SoftDelete(T2, actor);

        db.Users.Add(deletedUser);
        await db.SaveChangesAsync();

        var normalQuery = await db.Users.ToListAsync();
        var ignoredFilterQuery = await db.Users.IgnoreQueryFilters().ToListAsync();

        normalQuery.Should().BeEmpty();
        ignoredFilterQuery.Should().ContainSingle();
        ignoredFilterQuery.Single().IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_ReturnZero_When_NoChangesExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var changes = await db.SaveChangesAsync();

        changes.Should().Be(0);
    }
}