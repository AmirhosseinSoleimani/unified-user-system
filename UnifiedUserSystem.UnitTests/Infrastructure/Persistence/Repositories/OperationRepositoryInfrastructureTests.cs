using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories;

public class OperationRepositoryInfrastructureTests
{
    private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);

    [Fact]
    public async Task Add_Should_PersistOperationWithNormalizedValuesAndAudit_When_SaveChangesIsCalled()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new OperationRepository(db);
        var operation = InfrastructureDbContextFactory.CreateOperation(" Users.Read ", " Read Users ", T1);

        repository.Add(operation);
        await db.SaveChangesAsync();

        var persisted = await db.Operation.SingleAsync();
        persisted.Id.Should().NotBe(Guid.Empty);
        persisted.Key.Should().Be("users.read");
        persisted.Title.Should().Be("Read Users");
        persisted.IsActive.Should().BeTrue();
        persisted.IsDeleted.Should().BeFalse();
        persisted.CreatedAt.Should().Be(T1);
        persisted.UpdatedAt.Should().Be(T1);
    }

    [Fact]
    public async Task ListAsync_Should_ReturnOperationsOrderedByKey()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        db.Operation.AddRange(
            InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1),
            InfrastructureDbContextFactory.CreateOperation("role.read", "Read roles", T1),
            InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1));

        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var operations = await repository.ListAsync();

        operations.Select(x => x.Key).Should().Equal("role.read", "users.read", "users.update");
    }

    [Fact]
    public async Task ListAsync_Should_ExcludeSoftDeletedOperations()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var active = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        var deleted = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1);
        deleted.Delete(T2, null);

        db.Operation.AddRange(active, deleted);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var operations = await repository.ListAsync();

        operations.Should().ContainSingle();
        operations.Single().Key.Should().Be("users.read");
    }

    [Fact]
    public async Task ListAsync_Should_IncludeInactiveOperations_When_NotDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var inactive = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        inactive.Deactive(T2, null);

        db.Operation.Add(inactive);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var operations = await repository.ListAsync();

        operations.Should().ContainSingle();
        operations.Single().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_Should_ReturnAsNoTrackingResults()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using (var seedContext = InfrastructureDbContextFactory.Create(databaseName: databaseName))
        {
            seedContext.Operation.Add(InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1));
            await seedContext.SaveChangesAsync();
        }

        await using var db = InfrastructureDbContextFactory.Create(databaseName: databaseName);
        var repository = new OperationRepository(db);

        var operations = await repository.ListAsync();

        operations.Should().ContainSingle();
        db.ChangeTracker.Entries<Operation>().Should().BeEmpty();
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnOperation_When_OperationExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var found = await repository.FindByIdAsync(operation.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(operation.Id);
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnNull_When_OperationDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new OperationRepository(db);

        var found = await repository.FindByIdAsync(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnNull_When_OperationIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        operation.Delete(T2, null);

        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var found = await repository.FindByIdAsync(operation.Id);

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByKeyAsync_Should_ReturnOperation_When_KeyExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var found = await repository.FindByKeyAsync("users.read");

        found.Should().NotBeNull();
        found!.Id.Should().Be(operation.Id);
    }

    [Fact]
    public async Task FindByKeyAsync_Should_ReturnNull_When_KeyDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new OperationRepository(db);

        var found = await repository.FindByKeyAsync("missing.operation");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByKeyAsync_Should_ReturnNull_When_OperationIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        operation.Delete(T2, null);

        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var found = await repository.FindByKeyAsync("users.read");

        found.Should().BeNull();
    }

    [Fact]
    public async Task HasAssignedRolesAsync_Should_ReturnTrue_When_OperationIsAssignedToRole()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);

        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var hasRoles = await repository.HasAssignedRolesAsync(operation.Id);

        hasRoles.Should().BeTrue();
    }

    [Fact]
    public async Task HasAssignedRolesAsync_Should_ReturnFalse_When_OperationIsNotAssignedToAnyRole()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var hasRoles = await repository.HasAssignedRolesAsync(operation.Id);

        hasRoles.Should().BeFalse();
    }

    [Fact]
    public async Task HasAssignedRolesAsync_Should_ReturnFalse_When_AssignmentBelongsToAnotherOperation()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var target = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        var other = InfrastructureDbContextFactory.CreateOperation("users.update", "Update users", T1);

        db.Roles.Add(role);
        db.Operation.AddRange(target, other);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, other.Id, T1, null));
        await db.SaveChangesAsync();

        var repository = new OperationRepository(db);

        var hasRoles = await repository.HasAssignedRolesAsync(target.Id);

        hasRoles.Should().BeFalse();
    }

    [Fact]
    public async Task Update_Should_PersistTitleChange_When_SaveChangesIsCalled()
    {
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock);

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        operation.RenameTitle("Read all users", T2, null);
        await db.SaveChangesAsync();

        var persisted = await db.Operation.SingleAsync();

        persisted.Title.Should().Be("Read all users");
        persisted.UpdatedAt.Should().Be(T2);
    }

    [Fact]
    public async Task Update_Should_PersistKeyChange_When_SaveChangesIsCalled()
    {
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock);

        var operation = InfrastructureDbContextFactory.CreateOperation("users.read", "Read users", T1);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        operation.ChangeKey(" Users.List ", T2, null);
        await db.SaveChangesAsync();

        var persisted = await db.Operation.SingleAsync();

        persisted.Key.Should().Be("users.list");
        persisted.UpdatedAt.Should().Be(T2);
    }
}