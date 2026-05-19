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
    public class RoleOperationRepositoryTests
    {
        [Fact]
        public async Task ListByRoleIdAsync_Should_ReturnAssignedOperationsOrderedByOperationKey()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operationB = Operation.Create("users.update", "Update Users", now, actorUserId);
            var operationA = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.AddRange(operationB, operationA);
            await dbContext.SaveChangesAsync();

            dbContext.RoleOperations.AddRange(
                RoleOperation.Create(role.Id, operationB.Id, now, actorUserId),
                RoleOperation.Create(role.Id, operationA.Id, now, actorUserId));

            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);

            // Act
            var result = await sut.ListByRoleIdAsync(role.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Operation.Key).Should().Equal("users.read", "users.update");
        }

        [Fact]
        public async Task AddAsync_Should_PersistRoleOperation_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var roleOperation = RoleOperation.Create(role.Id, operation.Id, now, actorUserId);
            var sut = new RoleOperationRepository(dbContext);

            // Act
            await sut.AddAsync(roleOperation);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.RoleOperations.SingleAsync();
            persisted.RoleId.Should().Be(role.Id);
            persisted.OperationId.Should().Be(operation.Id);
        }

        [Fact]
        public async Task ExistsAsync_Should_ReturnTrue_When_RoleOperationExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            dbContext.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);

            // Act
            var result = await sut.ExistsAsync(role.Id, operation.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task FindAsync_Should_ReturnRoleOperationWithOperation_When_AssignmentExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            dbContext.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);

            // Act
            var result = await sut.FindAsync(role.Id, operation.Id);

            // Assert
            result.Should().NotBeNull();
            result!.RoleId.Should().Be(role.Id);
            result.OperationId.Should().Be(operation.Id);
            result.Operation.Should().NotBeNull();
            result.Operation.Key.Should().Be("users.read");
        }

        [Fact]
        public async Task Remove_Should_DeleteRoleOperation_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var roleOperation = RoleOperation.Create(role.Id, operation.Id, now, actorUserId);
            dbContext.RoleOperations.Add(roleOperation);
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);

            // Act
            sut.Remove(roleOperation);
            await dbContext.SaveChangesAsync();

            // Assert
            dbContext.RoleOperations.Should().BeEmpty();
        }

        [Fact]
        public async Task DuplicateAssign_Should_NotCreateDuplicateRows_When_ServiceChecksExistsBeforeAdd()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);

            if (!await sut.ExistsAsync(role.Id, operation.Id))
            {
                await sut.AddAsync(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
                await dbContext.SaveChangesAsync();
            }

            // Act
            if (!await sut.ExistsAsync(role.Id, operation.Id))
            {
                await sut.AddAsync(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
                await dbContext.SaveChangesAsync();
            }

            // Assert
            dbContext.RoleOperations.Should().ContainSingle();
        }

        [Fact]
        public async Task ReplaceAll_Should_PersistExactlyRequestedOperations()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var oldOperation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var newOperationA = Operation.Create("users.update", "Update Users", now, actorUserId);
            var newOperationB = Operation.Create("roles.read", "Read Roles", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.AddRange(oldOperation, newOperationA, newOperationB);
            await dbContext.SaveChangesAsync();

            dbContext.RoleOperations.Add(RoleOperation.Create(role.Id, oldOperation.Id, now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);
            var currentRoleOperations = await sut.ListByRoleIdAsync(role.Id);

            foreach (var roleOperation in currentRoleOperations)
                sut.Remove(roleOperation);

            await sut.AddAsync(RoleOperation.Create(role.Id, newOperationA.Id, now, actorUserId));
            await sut.AddAsync(RoleOperation.Create(role.Id, newOperationB.Id, now, actorUserId));

            // Act
            await dbContext.SaveChangesAsync();

            // Assert
            var result = await sut.ListByRoleIdAsync(role.Id);
            result.Select(x => x.OperationId).Should().BeEquivalentTo(new[] { newOperationA.Id, newOperationB.Id });
            result.Select(x => x.OperationId).Should().NotContain(oldOperation.Id);
        }

        [Fact]
        public async Task EmptyReplace_Should_RemoveAllOperationsForRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);

            var role = Role.Create("admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            dbContext.Roles.Add(role);
            dbContext.Operation.Add(operation);
            await dbContext.SaveChangesAsync();

            dbContext.RoleOperations.Add(RoleOperation.Create(role.Id, operation.Id, now, actorUserId));
            await dbContext.SaveChangesAsync();

            var sut = new RoleOperationRepository(dbContext);
            var currentRoleOperations = await sut.ListByRoleIdAsync(role.Id);

            foreach (var roleOperation in currentRoleOperations)
                sut.Remove(roleOperation);

            // Act
            await dbContext.SaveChangesAsync();

            // Assert
            var result = await sut.ListByRoleIdAsync(role.Id);
            result.Should().BeEmpty();
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
