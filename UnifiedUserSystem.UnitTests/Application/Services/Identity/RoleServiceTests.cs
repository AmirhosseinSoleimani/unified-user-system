using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class RoleServiceTests
    {
        [Fact]
        public async Task ListRolesAsync_Should_ReturnRolesFromRepository()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var roles = new List<Role>
        {
            Role.Create("admin", "Admin", now, actorUserId),
            Role.Create("support", "Support", now, actorUserId)
        };

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.ListAsync(ct))
                .ReturnsAsync(roles);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.ListRolesAsync(ct);

            // Assert
            result.Should().BeEquivalentTo(roles);
            roleRepositoryMock.Verify(x => x.ListAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_Should_AddRoleAndSaveChanges_When_NameIsValid()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            Role? capturedRole = null;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByNameAsync("Admin", ct))
                .ReturnsAsync((Role?)null);
            roleRepositoryMock
                .Setup(x => x.ExistsByKeyAsync("admin", ct))
                .ReturnsAsync(false);
            roleRepositoryMock
                .Setup(x => x.Add(It.IsAny<Role>()))
                .Callback<Role>(role => capturedRole = role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.CreateRoleAsync(" Admin ", ct);

            // Assert
            capturedRole.Should().NotBeNull();
            capturedRole.Should().BeSameAs(result);
            result.Name.Should().Be("Admin");
            result.Key.Should().Be("admin");
            result.IsActive.Should().BeTrue();

            roleRepositoryMock.Verify(x => x.FindByNameAsync("Admin", ct), Times.Once);
            roleRepositoryMock.Verify(x => x.ExistsByKeyAsync("admin", ct), Times.Once);
            roleRepositoryMock.Verify(x => x.Add(It.IsAny<Role>()), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_Should_ThrowDomainException_When_NameIsEmpty()
        {
            // Arrange
            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByNameAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.CreateRoleAsync(" ", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            roleRepositoryMock.Verify(x => x.Add(It.IsAny<Role>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateRoleAsync_Should_ThrowInvalidOperationException_When_NameAlreadyExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var existingRole = Role.Create("admin", "Admin", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByNameAsync("Admin", ct))
                .ReturnsAsync(existingRole);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.CreateRoleAsync("Admin", ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Role name already exists.");

            roleRepositoryMock.Verify(x => x.Add(It.IsAny<Role>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_RenameRoleAndSaveChanges_When_RequestIsValid()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync(role);
            roleRepositoryMock
                .Setup(x => x.FindByNameAsync("Administrator", ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now.AddMinutes(1)).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.UpdateRoleAsync(12, " Administrator ", ct);

            // Assert
            result.Should().BeSameAs(role);
            result.Name.Should().Be("Administrator");

            roleRepositoryMock.Verify(x => x.FindByIdAsync(12, ct), Times.Once);
            roleRepositoryMock.Verify(x => x.FindByNameAsync("Administrator", ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_ThrowDomainException_When_RoleIdIsInvalid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateRoleAsync(0, "Admin", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_ThrowDomainException_When_NameIsEmpty()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateRoleAsync(12, " ", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Role name is required.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateRoleAsync(12, "Administrator", ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_ThrowInvalidOperationException_When_NameBelongsToAnotherRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var existingRole = Role.Create("administrator", "Administrator", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync(role);
            roleRepositoryMock
                .Setup(x => x.FindByNameAsync("Administrator", ct))
                .ReturnsAsync(existingRole);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.UpdateRoleAsync(12, "Administrator", ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Role name already exists.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_Should_SoftDeleteRoleAndSaveChanges_When_RoleIsNotAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync(role);
            roleRepositoryMock
                .Setup(x => x.HasAssignedUsersAsync(12, ct))
                .ReturnsAsync(false);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now.AddMinutes(1)).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.DeleteRoleAsync(12, ct);

            // Assert
            role.IsDeleted.Should().BeTrue();
            roleRepositoryMock.Verify(x => x.FindByIdAsync(12, ct), Times.Once);
            roleRepositoryMock.Verify(x => x.HasAssignedUsersAsync(12, ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleAsync_Should_ThrowDomainException_When_RoleIdIsInvalid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteRoleAsync(0, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteRoleAsync(12, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            roleRepositoryMock.Verify(x => x.HasAssignedUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_Should_ThrowInvalidOperationException_When_RoleIsAssignedToUsers()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(12, ct))
                .ReturnsAsync(role);
            roleRepositoryMock
                .Setup(x => x.HasAssignedUsersAsync(12, ct))
                .ReturnsAsync(true);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(unitOfWorkMock.Object, CreateClockMock(now).Object, CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.DeleteRoleAsync(12, ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Role is assigned to users and cannot be deleted.");

            role.IsDeleted.Should().BeFalse();
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
