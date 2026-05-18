using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories
{
    public class RoleRepositoryTests
    {
        [Fact]
        public async Task ListAsync_Should_ReturnRolesOrderedByName()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            dbContext.Roles.AddRange(
                Role.Create("support", "Support", now, actorUserId),
                Role.Create("admin", "Admin", now, actorUserId),
                Role.Create("operator", "Operator", now, actorUserId));

            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.ListAsync();

            // Assert
            result.Select(x => x.Name).Should().Equal("Admin", "Operator", "Support");
        }

        [Fact]
        public async Task Add_Should_PersistRole_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);
            var sut = new RoleRepository(dbContext);

            var role = Role.Create("admin", "Admin", now, actorUserId);

            // Act
            sut.Add(role);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.Roles.SingleAsync();
            persisted.Name.Should().Be("Admin");
            persisted.Key.Should().Be("admin");
            persisted.IsActive.Should().BeTrue();
            persisted.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task FindByIdAsync_Should_ReturnRole_When_RoleExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.FindByIdAsync(role.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Admin");
        }

        [Fact]
        public async Task FindByNameAsync_Should_ReturnRole_When_NameExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            dbContext.Roles.Add(Role.Create("admin", "Admin", now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.FindByNameAsync("Admin");

            // Assert
            result.Should().NotBeNull();
            result!.Key.Should().Be("admin");
        }

        [Fact]
        public async Task ExistsByKeyAsync_Should_ReturnTrue_When_KeyExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            dbContext.Roles.Add(Role.Create("admin", "Admin", now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.ExistsByKeyAsync("admin");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Update_Should_PersistRoleNameChange_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            // Act
            role.Rename("Administrator", now.AddMinutes(1), actorUserId);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.Roles.SingleAsync();
            persisted.Name.Should().Be("Administrator");
            persisted.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_Should_SoftDeleteRole_When_RoleIsSafeToDelete()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);
            var hasAssignedUsers = await sut.HasAssignedUsersAsync(role.Id);
            hasAssignedUsers.Should().BeFalse();

            // Act
            role.Delete(now.AddMinutes(1), actorUserId);
            await dbContext.SaveChangesAsync();

            // Assert
            var visibleRoles = await dbContext.Roles.ToListAsync();
            visibleRoles.Should().BeEmpty();

            var deletedRole = await dbContext.Roles
                .IgnoreQueryFilters()
                .SingleAsync();

            deletedRole.IsDeleted.Should().BeTrue();
            deletedRole.Name.Should().Be("Admin");
        }

        [Fact]
        public async Task HasAssignedUsersAsync_Should_ReturnTrue_When_RoleIsAssignedToUser()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var user = User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice Doe",
                "initial-password-hash",
                now,
                actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);

            dbContext.Users.Add(user);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            var userRole = UserRole.Create(user.Id, role.Id, now, actorUserId);
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.HasAssignedUsersAsync(role.Id);

            // Assert
            result.Should().BeTrue();

            var userRoles = await dbContext.UserRoles.ToListAsync();
            userRoles.Should().ContainSingle();
            userRoles.Single().UserId.Should().Be(user.Id);
            userRoles.Single().RoleId.Should().Be(role.Id);
        }

        [Fact]
        public async Task HasAssignedUsersAsync_Should_ReturnFalse_When_RoleIsNotAssignedToUser()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            var sut = new RoleRepository(dbContext);

            // Act
            var result = await sut.HasAssignedUsersAsync(role.Id);

            // Assert
            result.Should().BeFalse();
            dbContext.UserRoles.Should().BeEmpty();
        }

        private static AppDbContext CreateDbContext(DateTimeOffset now, Guid actorUserId)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            return new AppDbContext(options, currentUserMock.Object, clockMock.Object);
        }
    }
}
