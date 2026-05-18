using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories
{
    public class OperationRepositoryTests
    {
        [Fact]
        public async Task ListAsync_Should_ReturnOperationsOrderedByKey()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            dbContext.Operation.AddRange(
                Operation.Create("role.update", "Update Roles", now, actorUserId),
                Operation.Create("role.create", "Create Roles", now, actorUserId),
                Operation.Create("role.delete", "Delete Roles", now, actorUserId));

            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);

            // Act
            var result = await sut.ListAsync();

            // Assert
            result.Select(x => x.Key).Should().Equal("role.create", "role.delete", "role.update");
        }

        [Fact]
        public async Task Add_Should_PersistOperation_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);
            var sut = new OperationRepository(dbContext);

            var operation = Operation.Create("role.create", "Create Roles", now, actorUserId);

            // Act
            sut.Add(operation);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.Operation.SingleAsync();
            persisted.Key.Should().Be("role.create");
            persisted.Title.Should().Be("Create Roles");
            persisted.IsActive.Should().BeTrue();
            persisted.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task FindByIdAsync_Should_ReturnOperation_When_OperationExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);

            // Act
            var result = await sut.FindByIdAsync(operation.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Key.Should().Be("role.read");
            result.Title.Should().Be("Read Roles");
        }

        [Fact]
        public async Task FindByKeyAsync_Should_ReturnOperation_When_KeyExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            dbContext.Operation.Add(Operation.Create("role.read", "Read Roles", now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);

            // Act
            var result = await sut.FindByKeyAsync("role.read");

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Read Roles");
        }

        [Fact]
        public async Task Update_Should_PersistOperationChanges_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            // Act
            operation.ChangeKey("role.update", now.AddMinutes(1), actorUserId);
            operation.RenameTitle("Update Roles", now.AddMinutes(1), actorUserId);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.Operation.SingleAsync();
            persisted.Key.Should().Be("role.update");
            persisted.Title.Should().Be("update roles");
            persisted.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_Should_SoftDeleteOperation_When_OperationIsSafeToDelete()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var operation = Operation.Create("role.delete", "Delete Roles", now, actorUserId);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);
            var hasAssignedRoles = await sut.HasAssignedRolesAsync(operation.Id);
            hasAssignedRoles.Should().BeFalse();

            // Act
            operation.Delete(now.AddMinutes(1), actorUserId);
            await dbContext.SaveChangesAsync();

            // Assert
            var visibleOperations = await dbContext.Operation.ToListAsync();
            visibleOperations.Should().BeEmpty();

            var deletedOperation = await dbContext.Operation
                .IgnoreQueryFilters()
                .SingleAsync();

            deletedOperation.IsDeleted.Should().BeTrue();
            deletedOperation.Key.Should().Be("role.delete");
        }

        [Fact]
        public async Task HasAssignedRolesAsync_Should_ReturnTrue_When_OperationIsAssignedToRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var roleOperation = RoleOperation.Create(role.Id, operation.Id, now, actorUserId);
            dbContext.RoleOperations.Add(roleOperation);
            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);

            // Act
            var result = await sut.HasAssignedRolesAsync(operation.Id);

            // Assert
            result.Should().BeTrue();

            var roleOperations = await dbContext.RoleOperations.ToListAsync();
            roleOperations.Should().ContainSingle();
            roleOperations.Single().RoleId.Should().Be(role.Id);
            roleOperations.Single().OperationId.Should().Be(operation.Id);
        }

        [Fact]
        public async Task HasAssignedRolesAsync_Should_ReturnFalse_When_OperationIsNotAssignedToRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var sut = new OperationRepository(dbContext);

            // Act
            var result = await sut.HasAssignedRolesAsync(operation.Id);

            // Assert
            result.Should().BeFalse();
            dbContext.RoleOperations.Should().BeEmpty();
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
