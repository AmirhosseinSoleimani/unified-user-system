using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Authorization;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Authorization
{
    public class EfPermissionReadRepositoryTests
    {
        [Fact]
        public async Task EfPermissionReadRepository_UserHasOperationAsync_WhenUserHasRoleWithOperation_ShouldReturnTrue()
        {
            await using var db = CreateDbContext();
            var seed = await SeedUserRoleOperationAsync(db, "users.read");

            var sut = new EfPermissionReadRepository(db);

            var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EfPermissionReadRepository_UserHasOperationAsync_WhenUserDoesNotHaveOperation_ShouldReturnFalse()
        {
            await using var db = CreateDbContext();
            var seed = await SeedUserRoleOperationAsync(db, "users.read");

            var sut = new EfPermissionReadRepository(db);

            var result = await sut.UserHasOperationAsync(seed.User.Id, "role.read");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task EfPermissionReadRepository_UserHasOperationAsync_WhenUserHasMultipleRoles_ShouldReturnTrueIfAnyRoleAllows()
        {
            await using var db = CreateDbContext();
            var now = DateTimeOffset.UtcNow;
            var actorUserId = Guid.NewGuid();

            var user = User.CreateNew("user@example.com", "user123", "Test User", "hashed-password", now, actorUserId);
            var roleWithoutPermission = Role.Create("viewer", "Viewer", now, actorUserId);
            var roleWithPermission = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("operation.read", "Read Operations", now, actorUserId);

            db.Users.Add(user);
            db.Roles.AddRange(roleWithoutPermission, roleWithPermission);
            db.Operation.Add(operation);
            await db.SaveChangesAsync();

            db.UserRoles.Add(UserRole.Create(user.Id, roleWithoutPermission.Id, now, actorUserId));
            db.UserRoles.Add(UserRole.Create(user.Id, roleWithPermission.Id, now, actorUserId));
            db.RoleOperations.Add(RoleOperation.Create(roleWithPermission.Id, operation.Id, now, actorUserId));
            await db.SaveChangesAsync();

            var sut = new EfPermissionReadRepository(db);

            var result = await sut.UserHasOperationAsync(user.Id, "operation.read");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EfPermissionReadRepository_UserHasOperationAsync_WhenUserInactive_ShouldReturnFalseIfSupportedByDomain()
        {
            await using var db = CreateDbContext();
            var seed = await SeedUserRoleOperationAsync(db, "users.read");
            seed.User.Deactive(DateTimeOffset.UtcNow, seed.User.Id);
            await db.SaveChangesAsync();

            var sut = new EfPermissionReadRepository(db);

            var result = await sut.UserHasOperationAsync(seed.User.Id, "users.read");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task EfPermissionReadRepository_UserHasOperationAsync_WhenOperationMissing_ShouldReturnFalse()
        {
            await using var db = CreateDbContext();
            var user = User.CreateNew("user@example.com", "user123", "Test User", "hashed-password", DateTimeOffset.UtcNow, Guid.NewGuid());
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new EfPermissionReadRepository(db);

            var result = await sut.UserHasOperationAsync(user.Id, "missing.operation");

            result.Should().BeFalse();
        }

        private static async Task<(User User, Role Role, Operation Operation)> SeedUserRoleOperationAsync(AppDbContext db, string operationKey)
        {
            var now = DateTimeOffset.UtcNow;
            var actorUserId = Guid.NewGuid();

            var user = User.CreateNew("user@example.com", "user123", "Test User", "hashed-password", now, actorUserId);
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create(operationKey, "Operation", now, actorUserId);

            db.Users.Add(user);
            db.Roles.Add(role);
            db.Operation.Add(operation);
            await db.SaveChangesAsync();

            db.UserRoles.Add(UserRole.Create(user.Id, role.Id, now, actorUserId));
            db.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
            await db.SaveChangesAsync();

            return (user, role, operation);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var currentUser = new Mock<ICurrentUser>();
            currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);

            var clock = new Mock<IClock>();
            clock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            return new AppDbContext(options, currentUser.Object, clock.Object);
        }
    }
}