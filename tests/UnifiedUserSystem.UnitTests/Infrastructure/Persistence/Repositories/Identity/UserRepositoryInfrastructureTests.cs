using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Identity;

public class UserRepositoryInfrastructureTests
{
    private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);

    [Fact]
    public async Task Add_Should_TrackAndPersistUserWithNormalizedValuesAndAudit_When_SaveChangesIsCalled()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new UserRepository(db);

        var user = InfrastructureDbContextFactory.CreateUser(
        email: " TEST@Example.com ",
        username: " testuser ",
        firstName: " Test ",
        lastName: " User ",
        phoneNumber: " +989123456789 ",
        passwordHash: "HASH",
        nowUtc: T1);

        repository.Add(user);
        await db.SaveChangesAsync();

        var persisted = await db.Users.SingleAsync();
        persisted.Id.Should().NotBe(Guid.Empty);
        persisted.Email.Should().Be("test@example.com");
        persisted.Username.Should().Be("testuser");
        persisted.FirstName.Should().Be("Test");
        persisted.LastName.Should().Be("User");
        persisted.PhoneNumber.Should().Be("+989123456789");
        persisted.Fullname.Should().Be("Test User");
        persisted.IsActive.Should().BeTrue();
        persisted.IsDeleted.Should().BeFalse();
        persisted.CreatedAt.Should().Be(T1);
        persisted.UpdatedAt.Should().Be(T1);
    }

    [Fact]
    public async Task EmailExistsAsync_Should_ReturnTrue_When_NormalizedEmailExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        db.Users.Add(InfrastructureDbContextFactory.CreateUser("test@example.com", "testuser", nowUtc: T1));
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var exists = await repository.EmailExistsAsync("test@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_Should_ReturnFalse_When_EmailDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new UserRepository(db);

        var exists = await repository.EmailExistsAsync("missing@example.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_Should_ReturnFalse_When_InputIsNotNormalized()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        db.Users.Add(InfrastructureDbContextFactory.CreateUser("test@example.com", "testuser", nowUtc: T1));
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var exists = await repository.EmailExistsAsync("TEST@example.com");

        exists.Should().BeFalse("repository expects normalized email input; normalization belongs to application/business layer");
    }

    [Fact]
    public async Task EmailExistsAsync_Should_ReturnFalse_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        user.SoftDelete(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var exists = await repository.EmailExistsAsync("deleted@example.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_Should_ReturnTrue_When_UsernameExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        db.Users.Add(InfrastructureDbContextFactory.CreateUser("ali@example.com", "aliuser", nowUtc: T1));
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var exists = await repository.UsernameExistsAsync("aliuser");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameExistsAsync_Should_ReturnFalse_When_UsernameDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new UserRepository(db);

        var exists = await repository.UsernameExistsAsync("missinguser");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_Should_ReturnFalse_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        user.SoftDelete(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var exists = await repository.UsernameExistsAsync("deleteduser");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_FindUserByEmail_AndIncludeRoles()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = InfrastructureDbContextFactory.CreateUser("ali@example.com", "aliuser", nowUtc: T1);
        user.AssignRole(role.Id, T1, user.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("ali@example.com");

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
        found.UserRoles.Should().ContainSingle();
        found.UserRoles.Single().RoleId.Should().Be(role.Id);
        found.UserRoles.Single().Role.Should().NotBeNull();
        found.UserRoles.Single().Role!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_FindUserByUsername_When_UsernameInputIsNormalized()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("ali@example.com", "aliuser", nowUtc: T1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("aliuser");

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_FindUserByUsername_When_UsernameInputHasDifferentCase()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("ali@example.com", "aliuser", nowUtc: T1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("ALIUSER");

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_ReturnNull_When_UserDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("missing@example.com");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_ReturnNull_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        user.SoftDelete(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("deleted@example.com");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindEmailOrUsernameAsync_Should_ReturnInactiveUser_When_UserIsInactiveButNotDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("inactive@example.com", "inactiveuser", nowUtc: T1);
        user.Deactive(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindEmailOrUsernameAsync("inactive@example.com");

        found.Should().NotBeNull();
        found!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnUserWithRoles_When_UserExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var role = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = InfrastructureDbContextFactory.CreateUser("user@example.com", "userone", nowUtc: T1);
        user.AssignRole(role.Id, T1, user.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindByIdAsync(user.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
        found.UserRoles.Should().ContainSingle();
        found.UserRoles.Single().Role.Should().NotBeNull();
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnNull_When_UserDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new UserRepository(db);

        var found = await repository.FindByIdAsync(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnNull_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        user.SoftDelete(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindByIdAsync(user.Id);

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdWithRolesAsync_Should_ReturnUserWithRoles_When_UserExists()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var admin = InfrastructureDbContextFactory.CreateRole("admin", "Admin", T1);
        var support = InfrastructureDbContextFactory.CreateRole("support", "Support", T1);

        db.Roles.AddRange(admin, support);
        await db.SaveChangesAsync();

        var user = InfrastructureDbContextFactory.CreateUser("roles@example.com", "rolesuser", nowUtc: T1);
        user.AssignRole(admin.Id, T1, user.Id);
        user.AssignRole(support.Id, T1, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindByIdWithRolesAsync(user.Id);

        found.Should().NotBeNull();
        found!.UserRoles.Should().HaveCount(2);
        found.UserRoles.Select(x => x.Role!.Name).Should().BeEquivalentTo("Admin", "Support");
    }

    [Fact]
    public async Task FindByIdWithRolesAsync_Should_ReturnNull_When_UserIsSoftDeleted()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var user = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        user.SoftDelete(T2, user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var found = await repository.FindByIdWithRolesAsync(user.Id);

        found.Should().BeNull();
    }

    [Fact]
    public async Task ListActiveAsync_Should_ReturnOnlyActiveUsersOrderedByUsername()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var b = InfrastructureDbContextFactory.CreateUser("b@example.com", "buser", nowUtc: T1);
        var a = InfrastructureDbContextFactory.CreateUser("a@example.com", "auser", nowUtc: T1);
        var inactive = InfrastructureDbContextFactory.CreateUser("inactive@example.com", "inactiveuser", nowUtc: T1);
        inactive.Deactive(T2, inactive.Id);

        db.Users.AddRange(b, inactive, a);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var users = await repository.ListActiveAsync();

        users.Should().HaveCount(2);
        users.Select(x => x.Username).Should().Equal("auser", "buser");
    }

    [Fact]
    public async Task ListActiveAsync_Should_ExcludeSoftDeletedUsers()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var active = InfrastructureDbContextFactory.CreateUser("active@example.com", "activeuser", nowUtc: T1);
        var deleted = InfrastructureDbContextFactory.CreateUser("deleted@example.com", "deleteduser", nowUtc: T1);
        deleted.SoftDelete(T2, deleted.Id);

        db.Users.AddRange(active, deleted);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var users = await repository.ListActiveAsync();

        users.Should().ContainSingle();
        users.Single().Username.Should().Be("activeuser");
    }

    [Fact]
    public async Task ListActiveAsync_Should_ReturnEmptyList_When_NoActiveUsersExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();

        var inactive = InfrastructureDbContextFactory.CreateUser("inactive@example.com", "inactiveuser", nowUtc: T1);
        inactive.Deactivate(T2, inactive.Id);

        db.Users.Add(inactive);
        await db.SaveChangesAsync();

        var repository = new UserRepository(db);

        var users = await repository.ListActiveAsync();

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_Should_PersistUserProfileChange_When_SaveChangesIsCalled()
    {
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        var user = InfrastructureDbContextFactory.CreateUser(
            email: "user@example.com",
            username: "userone",
            firstName: "Old",
            lastName: "Name",
            phoneNumber: "+989123456789",
            passwordHash: "HASH",
            nowUtc: T1);

        await using var db = InfrastructureDbContextFactory.Create(clock, currentUserId: user.Id);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        user.ChangeProfile(
            firstName: "New",
            lastName: "Name",
            phoneNumber: "09123456789",
            nowUtc: T2,
            actorUserId: user.Id);

        await db.SaveChangesAsync();

        var persisted = await db.Users.SingleAsync();

        persisted.FirstName.Should().Be("New");
        persisted.LastName.Should().Be("Name");
        persisted.PhoneNumber.Should().Be("09123456789");
        persisted.Fullname.Should().Be("New Name");
        persisted.UpdatedAt.Should().Be(T2);
        persisted.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Update_Should_PersistPasswordHashChange_When_SaveChangesIsCalled()
    {
        var clock = new InfrastructureTestClock { Utcnow = T1 };

        await using var db = InfrastructureDbContextFactory.Create(clock);

        var user = InfrastructureDbContextFactory.CreateUser(
            email: "user@example.com",
            username: "userone",
            firstName: "User",
            lastName: "One",
            phoneNumber: "+989123456789",
            passwordHash: "HASH-1",
            nowUtc: T1);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        clock.Utcnow = T2;

        user.ChangePasswordHash("HASH-2", T2, user.Id);
        await db.SaveChangesAsync();

        var persisted = await db.Users.SingleAsync();

        persisted.PasswordHash.Should().Be("HASH-2");
        persisted.FirstName.Should().Be("User");
        persisted.LastName.Should().Be("One");
        persisted.PhoneNumber.Should().Be("+989123456789");
        persisted.Fullname.Should().Be("User One");
        persisted.UpdatedAt.Should().Be(T2);
    }

    [Fact]
    public async Task Remove_Should_SoftDeleteUser_When_DbSetRemoveIsUsed()
    {
        await using var db = InfrastructureDbContextFactory.Create(currentUserId: Guid.NewGuid());

        var user = InfrastructureDbContextFactory.CreateUser("delete@example.com", "deleteuser", nowUtc: T1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var visibleUsers = await db.Users.ToListAsync();
        visibleUsers.Should().BeEmpty();

        var deletedUser = await db.Users.IgnoreQueryFilters().SingleAsync();
        deletedUser.IsDeleted.Should().BeTrue();
        deletedUser.DeletedAt.Should().NotBeNull();
        deletedUser.DeletedByUserId.Should().NotBeNull();
    }
}