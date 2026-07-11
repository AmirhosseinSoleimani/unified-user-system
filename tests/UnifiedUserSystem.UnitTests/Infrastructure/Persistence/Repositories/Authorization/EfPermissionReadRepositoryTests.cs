using FluentAssertions;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Authorization;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Authorization;

public class EfPermissionReadRepositoryTests
{
    private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnTrue_When_UserHasRoleWithOperation()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_NormalizeOperationPolicyPrefixAndCase()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "OP:USERS.READ");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserIdIsEmpty()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(Guid.Empty, "users.read");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_OperationKeyIsEmpty(string? operationKey)
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, operationKey!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(Guid.NewGuid(), "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserDoesNotHaveRequestedOperation()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "role.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnTrue_When_UserHasMultipleRolesAndAnyRoleAllowsOperation()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var user = CreateUser();
        var viewer = Role.Create("viewer", "Viewer", T1, null);
        var admin = Role.Create("admin", "Admin", T1, null);
        var operation = Operation.Create("operation.read", "Read Operations", T1, null);

        db.Users.Add(user);
        db.Roles.AddRange(viewer, admin);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.UserRoles.AddRange(
            UserRole.Create(user.Id, viewer.Id, T1, null),
            UserRole.Create(user.Id, admin.Id, T1, null));

        db.RoleOperations.Add(RoleOperation.Create(admin.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(user.Id, "operation.read");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserIsInactive()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.User.Deactivate(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_RoleIsInactive()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.Role.Deactivate(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_OperationIsInactive()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.Operation.Deactivate(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.User.SoftDelete(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_RoleIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.Role.Delete(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_OperationIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var seed = await SeedUserRoleOperationAsync(db, "users.read");

        seed.Operation.Delete(T2, seed.User.Id);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_UserHasRoleButRoleHasNoOperations()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var user = CreateUser();
        var role = Role.Create("admin", "Admin", T1, null);

        db.Users.Add(user);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        db.UserRoles.Add(UserRole.Create(user.Id, role.Id, T1, null));
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(user.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasOperationAsync_Should_ReturnFalse_When_RoleHasOperationButUserDoesNotHaveRole()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var user = CreateUser();
        var role = Role.Create("admin", "Admin", T1, null);
        var operation = Operation.Create("users.read", "Read users", T1, null);

        db.Users.Add(user);
        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.UserHasOperationAsync(user.Id, "users.read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ListUserIdsInRoleAsync_Should_ReturnDistinctUserIds_When_RoleHasUsers()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = Role.Create("admin", "Admin", T1, null);

        var user1 = CreateUser(
        email: "u1@example.com",
        username: "userone",
        firstName: "User",
        lastName: "One",
        phoneNumber: "+989111111111");

        var user2 = CreateUser(
            email: "u2@example.com",
            username: "usertwo",
            firstName: "User",
            lastName: "Two",
            phoneNumber: "+989222222222");

        db.Roles.Add(role);
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        db.UserRoles.AddRange(
            UserRole.Create(user1.Id, role.Id, T1, null),
            UserRole.Create(user2.Id, role.Id, T1, null));

        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.ListUserIdsInRoleAsync(role.Id);

        result.Should().BeEquivalentTo(new[] { user1.Id, user2.Id });
    }

    [Fact]
    public async Task ListUserIdsInRoleAsync_Should_ReturnEmptyList_When_RoleIdIsInvalid()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var sut = new EfPermissionReadRepository(db);

        var result = await sut.ListUserIdsInRoleAsync(0);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListUserIdsInRoleAsync_Should_ReturnEmptyList_When_RoleHasNoUsers()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = Role.Create("admin", "Admin", T1, null);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var sut = new EfPermissionReadRepository(db);

        var result = await sut.ListUserIdsInRoleAsync(role.Id);

        result.Should().BeEmpty();
    }

    private static async Task<(User User, Role Role, Operation Operation)> SeedUserRoleOperationAsync(
        UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.AppDbContext db,
        string operationKey)
    {
        var user = CreateUser();
        var role = Role.Create("admin", "Admin", T1, null);
        var operation = Operation.Create(operationKey, "Operation", T1, null);

        db.Users.Add(user);
        db.Roles.Add(role);
        db.Operation.Add(operation);
        await db.SaveChangesAsync();

        db.UserRoles.Add(UserRole.Create(user.Id, role.Id, T1, null));
        db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, T1, null));
        await db.SaveChangesAsync();

        return (user, role, operation);
    }

    private static User CreateUser(
    string email = "user@example.com",
    string username = "user123",
    string firstName = "Test",
    string lastName = "User",
    string phoneNumber = "+989123456789",
    string passwordHash = "hashed-password")
    {
        return User.CreateNew(
            email,
            username,
            firstName,
            lastName,
            phoneNumber,
            passwordHash,
            T1,
            null);
    }
}