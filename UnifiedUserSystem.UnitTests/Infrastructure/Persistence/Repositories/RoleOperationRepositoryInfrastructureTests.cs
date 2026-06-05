using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories;

public class RoleOperationRepositoryInfrastructureTests
{
    private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    [Fact]
    public async Task AddAsync_Should_PersistRoleOperation_When_SaveChangesIsCalled()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);
        var roleOperation = RoleOperation.Create(role.Id, operation.Id, T1, null);

        await repository.AddAsync(roleOperation);
        await db.SaveChangesAsync();

        var persisted = await db.RoleOperations.SingleAsync();

        persisted.RoleId.Should().Be(role.Id);
        persisted.OperationId.Should().Be(operation.Id);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_AssignmentExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var exists = await repository.ExistsAsync(role.Id, operation.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_AssignmentDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RoleOperationRepository(db);

        var exists = await repository.ExistsAsync(123, Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_RoleMatchesButOperationDoesNot()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var assigned = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        var target = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1);

        db.Roles.Add(role);
        db.Operation.AddRange(assigned, target);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, assigned.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var exists = await repository.ExistsAsync(role.Id, target.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_OperationMatchesButRoleDoesNot()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var otherRole = InfrastructureDbContextFactory.CreateRole("support", "Support", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.AddRange(role, otherRole);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(otherRole.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var exists = await repository.ExistsAsync(role.Id, operation.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FindAsync_Should_ReturnRoleOperationWithOperation_When_AssignmentExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var found = await repository.FindAsync(role.Id, operation.Id);

        found.Should().NotBeNull();
        found!.RoleId.Should().Be(role.Id);
        found.OperationId.Should().Be(operation.Id);
        found.Operation.Should().NotBeNull();
        found.Operation!.Key.Should().Be("users.read");
    }

    [Fact]
    public async Task FindAsync_Should_ReturnNull_When_AssignmentDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RoleOperationRepository(db);

        var found = await repository.FindAsync(123, Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task ListByRoleIdAsync_Should_ReturnOnlyRoleOperationsForRoleOrderedByOperationKey()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var otherRole = InfrastructureDbContextFactory.CreateRole("support", "Support", T1);

        var update = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1);
        var read = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        var other = InfrastructureDbContextFactory.CreateOperation("roles.read", "Read roles", T1);

        db.Roles.AddRange(role, otherRole);
        db.Operation.AddRange(update, read, other);
        await db.SaveChangesAsync();

        db.RoleOperations.AddRange(
            RoleOperation.Create(role.Id, update.Id, T1, null),
            RoleOperation.Create(role.Id, read.Id, T1, null),
            RoleOperation.Create(otherRole.Id, other.Id, T1, null));

        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var assignments = await repository.ListByRoleIdAsync(role.Id);

        assignments.Should().HaveCount(2);
        assignments.Select(x => x.Operation.Key).Should().Equal("users.read", "users.update");
        assignments.All(x => x.RoleId == role.Id).Should().BeTrue();
    }

    [Fact]
    public async Task ListByRoleIdAsync_Should_ReturnEmptyList_When_RoleHasNoOperations()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var assignments = await repository.ListByRoleIdAsync(role.Id);

        assignments.Should().BeEmpty();
    }

    [Fact]
    public async Task ListByRoleIdAsync_Should_IncludeOperationNavigationForEachAssignment()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        var assignments = await repository.ListByRoleIdAsync(role.Id);

        assignments.Should().ContainSingle();
        assignments.Single().Operation.Should().NotBeNull();
        assignments.Single().Operation!.Key.Should().Be("users.read");
    }

    [Fact]
    public async Task Remove_Should_DeleteOnlyTargetRoleOperation_When_SaveChangesIsCalled()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var read = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        var update = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1);

        db.Roles.Add(role);
        db.Operation.AddRange(read, update);
        await db.SaveChangesAsync();

        var readAssignment = RoleOperation.Create(role.Id, read.Id, T1, null);
        var updateAssignment = RoleOperation.Create(role.Id, update.Id, T1, null);

        db.RoleOperations.AddRange(readAssignment, updateAssignment);
        await db.SaveChangesAsync();

        var repository = new RoleOperationRepository(db);

        repository.Remove(readAssignment);
        await db.SaveChangesAsync();

        var remaining = await db.RoleOperations.SingleAsync();

        remaining.OperationId.Should().Be(update.Id);

        var readStillExists = await repository.ExistsAsync(role.Id, read.Id);
        var updateStillExists = await repository.ExistsAsync(role.Id, update.Id);

        readStillExists.Should().BeFalse();
        updateStillExists.Should().BeTrue();
    }
}