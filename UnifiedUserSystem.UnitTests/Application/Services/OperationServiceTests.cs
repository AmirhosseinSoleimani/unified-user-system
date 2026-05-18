using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services
{
    public class OperationServiceTests
    {
        [Fact]
        public async Task ListOperationsAsync_Should_ReturnOperationsFromRepository()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var operations = new List<Operation>
        {
            Operation.Create("role.read", "Read Roles", now, actorUserId),
            Operation.Create("role.create", "Create Roles", now, actorUserId)
        };

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.ListAsync(ct))
                .ReturnsAsync(operations);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.ListOperationsAsync(ct);

            // Assert
            result.Should().BeEquivalentTo(operations);
            operationRepositoryMock.Verify(x => x.ListAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CreateOperationAsync_Should_AddOperationAndSaveChanges_When_RequestIsValid()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            Operation? capturedOperation = null;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByKeyAsync("role.create", ct))
                .ReturnsAsync((Operation?)null);
            operationRepositoryMock
                .Setup(x => x.Add(It.IsAny<Operation>()))
                .Callback<Operation>(operation => capturedOperation = operation);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.CreateOperationAsync(" Role.Create ", " Create Roles ", ct);

            // Assert
            capturedOperation.Should().NotBeNull();
            capturedOperation.Should().BeSameAs(result);
            result.Key.Should().Be("role.create");
            result.Title.Should().Be("Create Roles");
            result.IsActive.Should().BeTrue();

            operationRepositoryMock.Verify(x => x.FindByKeyAsync("role.create", ct), Times.Once);
            operationRepositoryMock.Verify(x => x.Add(It.IsAny<Operation>()), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CreateOperationAsync_Should_ThrowDomainException_When_KeyIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.CreateOperationAsync(" ", "Create Roles", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Operation key is required.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOperationAsync_Should_ThrowDomainException_When_TitleIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.CreateOperationAsync("role.create", " ", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Operation title is required.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOperationAsync_Should_ThrowArgumentException_When_KeyContainsInvalidCharacters()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByKeyAsync("role create!", ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.CreateOperationAsync("role create!", "Create Roles", ct);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Operation key contains invalid characters.");

            operationRepositoryMock.Verify(x => x.Add(It.IsAny<Operation>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOperationAsync_Should_ThrowInvalidOperationException_When_KeyAlreadyExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var existingOperation = Operation.Create("role.create", "Create Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByKeyAsync("role.create", ct))
                .ReturnsAsync(existingOperation);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.CreateOperationAsync("role.create", "Create Roles", ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Operation key already exists.");

            operationRepositoryMock.Verify(x => x.Add(It.IsAny<Operation>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ChangeOperationAndSaveChanges_When_RequestIsValid()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);
            operationRepositoryMock
                .Setup(x => x.FindByKeyAsync("role.update", ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now.AddMinutes(1)).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.UpdateOperationAsync(operation.Id, " Role.Update ", " Update Roles ", ct);

            // Assert
            result.Should().BeSameAs(operation);
            result.Key.Should().Be("role.update");
            result.Title.Should().Be("update roles");

            operationRepositoryMock.Verify(x => x.FindByIdAsync(operation.Id, ct), Times.Once);
            operationRepositoryMock.Verify(x => x.FindByKeyAsync("role.update", ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ThrowDomainException_When_OperationIdIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateOperationAsync(Guid.Empty, "role.update", "Update Roles", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ThrowDomainException_When_KeyIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateOperationAsync(Guid.NewGuid(), " ", "Update Roles", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Operation key is required.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ThrowDomainException_When_TitleIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateOperationAsync(Guid.NewGuid(), "role.update", " ", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Operation title is required.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ThrowKeyNotFoundException_When_OperationDoesNotExist()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operationId, ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateOperationAsync(operationId, "role.update", "Update Roles", ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Operation not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOperationAsync_Should_ThrowInvalidOperationException_When_KeyBelongsToAnotherOperation()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            var existingOperation = Operation.Create("role.update", "Update Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);
            operationRepositoryMock
                .Setup(x => x.FindByKeyAsync("role.update", ct))
                .ReturnsAsync(existingOperation);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateOperationAsync(operation.Id, "role.update", "Update Roles", ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Operation key already exists.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOperationAsync_Should_SoftDeleteOperationAndSaveChanges_When_OperationIsNotAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.delete", "Delete Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);
            operationRepositoryMock
                .Setup(x => x.HasAssignedRolesAsync(operation.Id, ct))
                .ReturnsAsync(false);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now.AddMinutes(1)).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.DeleteOperationAsync(operation.Id, ct);

            // Assert
            operation.IsDeleted.Should().BeTrue();
            operationRepositoryMock.Verify(x => x.FindByIdAsync(operation.Id, ct), Times.Once);
            operationRepositoryMock.Verify(x => x.HasAssignedRolesAsync(operation.Id, ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeleteOperationAsync_Should_ThrowDomainException_When_OperationIdIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteOperationAsync(Guid.Empty, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOperationAsync_Should_ThrowKeyNotFoundException_When_OperationDoesNotExist()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operationId, ct))
                .ReturnsAsync((Operation?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteOperationAsync(operationId, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Operation not found.");

            operationRepositoryMock.Verify(x => x.HasAssignedRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOperationAsync_Should_ThrowInvalidOperationException_When_OperationIsAssignedToRoles()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.delete", "Delete Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationRepositoryMock = new Mock<IOperationRepository>();
            operationRepositoryMock
                .Setup(x => x.FindByIdAsync(operation.Id, ct))
                .ReturnsAsync(operation);
            operationRepositoryMock
                .Setup(x => x.HasAssignedRolesAsync(operation.Id, ct))
                .ReturnsAsync(true);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Operations).Returns(operationRepositoryMock.Object);

            var sut = new OperationService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteOperationAsync(operation.Id, ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Operation is assigned to roles and cannot be deleted.");

            operation.IsDeleted.Should().BeFalse();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
