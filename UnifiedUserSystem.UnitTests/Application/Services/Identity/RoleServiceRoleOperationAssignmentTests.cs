using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class RoleServiceRoleOperationAssignmentTests
    {
        [Fact]
        public async Task GetRoleOperationsAsync_Should_ReturnAssignedOperations_When_RoleExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var roleOperation = CreateRoleOperation(role.Id, operation, now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .Setup(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(new[] { roleOperation });

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.GetRoleOperationsAsync(10, ct);

            // Assert
            result.RoleId.Should().Be(10);
            result.Operations.Should().ContainSingle();
            result.Operations.Single().OperationId.Should().Be(operation.Id);
            result.Operations.Single().Key.Should().Be("users.read");
            result.Operations.Single().Title.Should().Be("Read Users");
            result.Operations.Single().IsActive.Should().BeTrue();

            roleRepositoryMock.Verify(x => x.FindByIdAsync(10, ct), Times.Once);
            roleOperationRepositoryMock.Verify(x => x.ListByRoleIdAsync(10, ct), Times.Once);
        }

        [Fact]
        public async Task GetRoleOperationsAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.GetRoleOperationsAsync(10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");
        }

        [Fact]
        public async Task AssignOperationToRoleAsync_Should_AddRoleOperationAndSaveChanges_When_NotAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);
            RoleOperation? addedRoleOperation = null;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .Setup(x => x.ExistsAsync(10, operation.Id, ct))
                .ReturnsAsync(false);
            roleOperationRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<RoleOperation>(), ct))
                .Callback<RoleOperation, CancellationToken>((roleOperation, _) => addedRoleOperation = roleOperation)
                .Returns(Task.CompletedTask);
            roleOperationRepositoryMock
                .Setup(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(() => addedRoleOperation is null
                    ? Array.Empty<RoleOperation>()
                    : new[] { AttachOperation(addedRoleOperation, operation) });

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.AssignOperationToRoleAsync(10, operation.Id, ct);

            // Assert
            addedRoleOperation.Should().NotBeNull();
            addedRoleOperation!.RoleId.Should().Be(10);
            addedRoleOperation.OperationId.Should().Be(operation.Id);

            result.RoleId.Should().Be(10);
            result.Operations.Should().ContainSingle(x => x.OperationId == operation.Id);

            roleOperationRepositoryMock.Verify(x => x.ExistsAsync(10, operation.Id, ct), Times.Once);
            roleOperationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RoleOperation>(), ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task AssignOperationToRoleAsync_Should_NotCreateDuplicateRows_When_AlreadyAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var roleOperation = CreateRoleOperation(role.Id, operation, now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .Setup(x => x.ExistsAsync(10, operation.Id, ct))
                .ReturnsAsync(true);
            roleOperationRepositoryMock
                .Setup(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(new[] { roleOperation });

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.AssignOperationToRoleAsync(10, operation.Id, ct);

            // Assert
            result.Operations.Should().ContainSingle(x => x.OperationId == operation.Id);
            roleOperationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RoleOperation>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignOperationToRoleAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.AssignOperationToRoleAsync(10, operationId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignOperationToRoleAsync_Should_ThrowKeyNotFoundException_When_OperationDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operationId, ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.AssignOperationToRoleAsync(10, operationId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Operation not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveOperationFromRoleAsync_Should_RemoveExistingRoleOperationAndSaveChanges()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var roleOperation = CreateRoleOperation(role.Id, operation, now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .Setup(x => x.FindAsync(10, operation.Id, ct))
                .ReturnsAsync(roleOperation);
            roleOperationRepositoryMock
                .Setup(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(Array.Empty<RoleOperation>());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.RemoveOperationFromRoleAsync(10, operation.Id, ct);

            // Assert
            result.RoleId.Should().Be(10);
            result.Operations.Should().BeEmpty();

            roleOperationRepositoryMock.Verify(x => x.Remove(roleOperation), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task RemoveOperationFromRoleAsync_Should_BeIdempotent_When_OperationIsNotAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .Setup(x => x.FindAsync(10, operation.Id, ct))
                .ReturnsAsync((RoleOperation?)null);
            roleOperationRepositoryMock
                .Setup(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(Array.Empty<RoleOperation>());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.RemoveOperationFromRoleAsync(10, operation.Id, ct);

            // Assert
            result.Operations.Should().BeEmpty();
            roleOperationRepositoryMock.Verify(x => x.Remove(It.IsAny<RoleOperation>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveOperationFromRoleAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.RemoveOperationFromRoleAsync(10, operationId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveOperationFromRoleAsync_Should_ThrowKeyNotFoundException_When_OperationDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operationId, ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.RemoveOperationFromRoleAsync(10, operationId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Operation not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_PersistExactlyRequestedOperations()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var existingOperation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var newOperationA = Operation.Create("users.update", "Update Users", now, actorUserId);
            var newOperationB = Operation.Create("roles.read", "Read Roles", now, actorUserId);

            var currentRoleOperation = CreateRoleOperation(role.Id, existingOperation, now, actorUserId);
            var finalRoleOperations = new List<RoleOperation>();

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock.Setup(x => x.FindByIdAsync(newOperationA.Id, ct)).ReturnsAsync(newOperationA);
            operationRepositoryMock.Setup(x => x.FindByIdAsync(newOperationB.Id, ct)).ReturnsAsync(newOperationB);

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .SetupSequence(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(new[] { currentRoleOperation })
                .ReturnsAsync(() => finalRoleOperations);
            roleOperationRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<RoleOperation>(), ct))
                .Callback<RoleOperation, CancellationToken>((roleOperation, _) =>
                {
                    if (roleOperation.OperationId == newOperationA.Id)
                        finalRoleOperations.Add(AttachOperation(roleOperation, newOperationA));
                    if (roleOperation.OperationId == newOperationB.Id)
                        finalRoleOperations.Add(AttachOperation(roleOperation, newOperationB));
                })
                .Returns(Task.CompletedTask);
            roleOperationRepositoryMock
                .Setup(x => x.Remove(currentRoleOperation))
                .Callback(() => finalRoleOperations.Remove(currentRoleOperation));

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.ReplaceRoleOperationsAsync(10, new[] { newOperationA.Id, newOperationB.Id }, ct);

            // Assert
            result.RoleId.Should().Be(10);
            result.Operations.Select(x => x.OperationId).Should().BeEquivalentTo(new[] { newOperationA.Id, newOperationB.Id });

            roleOperationRepositoryMock.Verify(x => x.Remove(currentRoleOperation), Times.Once);
            roleOperationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RoleOperation>(), ct), Times.Exactly(2));
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_RemoveAllOperations_When_OperationIdsIsEmpty()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            var operation = Operation.Create("users.read", "Read Users", now, actorUserId);
            var roleOperation = CreateRoleOperation(role.Id, operation, now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();

            var roleOperationRepositoryMock = new Mock<IRoleOperationRepository>();
            roleOperationRepositoryMock
                .SetupSequence(x => x.ListByRoleIdAsync(10, ct))
                .ReturnsAsync(new[] { roleOperation })
                .ReturnsAsync(Array.Empty<RoleOperation>());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.RoleOperations).Returns(roleOperationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.ReplaceRoleOperationsAsync(10, Array.Empty<Guid>(), ct);

            // Assert
            result.Operations.Should().BeEmpty();
            operationRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            roleOperationRepositoryMock.Verify(x => x.Remove(roleOperation), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_ThrowDomainException_When_OperationIdsIsNull()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceRoleOperationsAsync(10, null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("OperationIds is required.");
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_ThrowDomainException_When_OperationIdsContainsEmptyGuid()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceRoleOperationsAsync(10, new[] { Guid.Empty }, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("OperationId is invalid.");
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_ThrowDomainException_When_OperationIdsContainsDuplicates()
        {
            // Arrange
            var operationId = Guid.NewGuid();

            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceRoleOperationsAsync(10, new[] { operationId, operationId }, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Duplicate operation ids are not allowed.");
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceRoleOperationsAsync(10, new[] { operationId }, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceRoleOperationsAsync_Should_ThrowKeyNotFoundException_When_RequestedOperationDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operationId, ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceRoleOperationsAsync(10, new[] { operationId }, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Operation not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Role CreateRole(int id, string key, string name, DateTimeOffset now, Guid actorUserId)
        {
            var role = Role.Create(key, name, now, actorUserId);
            SetProperty(role, nameof(Role.Id), id);
            return role;
        }

        private static RoleOperation CreateRoleOperation(int roleId, Operation operation, DateTimeOffset now, Guid actorUserId)
        {
            var roleOperation = RoleOperation.Create(roleId, operation.Id, now, actorUserId);
            return AttachOperation(roleOperation, operation);
        }

        private static RoleOperation AttachOperation(RoleOperation roleOperation, Operation operation)
        {
            SetProperty(roleOperation, nameof(RoleOperation.Operation), operation);
            return roleOperation;
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(propertyName);
            property.Should().NotBeNull();
            property!.SetValue(target, value);
        }

        private static Mock<IClock> CreateClockMock(DateTimeOffset now)
        {
            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);
            return clockMock;
        }

        private static Mock<ICurrentUser> CreateCurrentUserMock(Guid actorUserId)
        {
            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
            return currentUserMock;
        }
    }
}
