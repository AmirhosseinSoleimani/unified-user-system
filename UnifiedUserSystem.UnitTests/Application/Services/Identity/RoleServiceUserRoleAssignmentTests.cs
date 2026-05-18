using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class RoleServiceUserRoleAssignmentTests
    {
        [Fact]
        public async Task AssignRoleToUserAsync_Should_AssignRoleAndSaveChanges_When_UserAndRoleExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.AssignRoleToUserAsync(user.Id, 10, ct);

            // Assert
            result.UserId.Should().Be(user.Id);
            user.UserRoles.Should().ContainSingle(x => x.RoleId == 10);
            user.UserRoles.Select(x => x.RoleId).Should().Equal(10);

            userRepositoryMock.Verify(x => x.FindByIdWithRolesAsync(user.Id, ct), Times.Once);
            roleRepositoryMock.Verify(x => x.FindByIdAsync(10, ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_NotCreateDuplicateRows_When_RoleIsAlreadyAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            user.AssignRole(10, now, actorUserId);
            user.UserRoles.Should().ContainSingle();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.AssignRoleToUserAsync(user.Id, 10, ct);

            // Assert
            user.UserRoles.Should().ContainSingle(x => x.RoleId == 10);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_ThrowDomainException_When_RoleIdIsInvalid()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.AssignRoleToUserAsync(Guid.NewGuid(), 0, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("RoleId is invalid.");
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_ThrowDomainException_When_UserIdIsInvalid()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.AssignRoleToUserAsync(Guid.Empty, 10, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("UserId is invalid.");
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_ThrowKeyNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(userId, ct))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.AssignRoleToUserAsync(userId, 10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.AssignRoleToUserAsync(user.Id, 10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            user.UserRoles.Should().BeEmpty();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Should_ThrowInvalidOperationException_When_RoleIsInactive()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            role.Deactivate(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.AssignRoleToUserAsync(user.Id, 10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Role is not active.");

            user.UserRoles.Should().BeEmpty();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_Should_RemoveAssignedRoleAndSaveChanges()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(10, "admin", "Admin", now, actorUserId);
            user.AssignRole(10, now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            var result = await sut.RemoveRoleFromUserAsync(user.Id, 10, ct);

            // Assert
            result.UserId.Should().Be(user.Id);
            user.UserRoles.Should().BeEmpty();

            userRepositoryMock.Verify(x => x.FindByIdWithRolesAsync(user.Id, ct), Times.Once);
            roleRepositoryMock.Verify(x => x.FindByIdAsync(10, ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_Should_BeIdempotent_When_RoleIsNotAssigned()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(10, "admin", "Admin", now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.RemoveRoleFromUserAsync(user.Id, 10, ct);

            // Assert
            user.UserRoles.Should().BeEmpty();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_Should_ThrowDomainException_When_RoleIdIsInvalid()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.RemoveRoleFromUserAsync(Guid.NewGuid(), 0, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("RoleId is invalid.");
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_Should_ThrowKeyNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(userId, ct))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.RemoveRoleFromUserAsync(userId, 10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_Should_ThrowKeyNotFoundException_When_RoleDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(10, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.RemoveRoleFromUserAsync(user.Id, 10, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_PersistExactlyRequestedRoles()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var admin = CreateRole(10, "admin", "Admin", now, actorUserId);
            var support = CreateRole(20, "support", "Support", now, actorUserId);
            var auditor = CreateRole(30, "auditor", "Auditor", now, actorUserId);

            user.AssignRole(10, now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(x => x.FindByIdAsync(10, ct)).ReturnsAsync(admin);
            roleRepositoryMock.Setup(x => x.FindByIdAsync(20, ct)).ReturnsAsync(support);
            roleRepositoryMock.Setup(x => x.FindByIdAsync(30, ct)).ReturnsAsync(auditor);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.ReplaceUserRolesAsync(user.Id, new[] { 20, 30 }, ct);

            // Assert
            user.UserRoles.Select(x => x.RoleId).Should().BeEquivalentTo(new[] { 20, 30 });
            user.UserRoles.Select(x => x.RoleId).Should().NotContain(10);

            roleRepositoryMock.Verify(x => x.FindByIdAsync(20, ct), Times.Once);
            roleRepositoryMock.Verify(x => x.FindByIdAsync(30, ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_RemoveAllRoles_When_RequestedRoleIdsIsEmpty()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            user.AssignRole(10, now, actorUserId);
            user.AssignRole(20, now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            await sut.ReplaceUserRolesAsync(user.Id, Array.Empty<int>(), ct);

            // Assert
            user.UserRoles.Should().BeEmpty();
            roleRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowDomainException_When_RoleIdsIsNull()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(Guid.NewGuid(), null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("RoleIds is required.");
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowDomainException_When_RoleIdsContainsDuplicates()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(Guid.NewGuid(), new[] { 10, 10 }, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Duplicate role ids are not allowed.");
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowDomainException_When_RoleIdsContainsInvalidRoleId()
        {
            // Arrange
            var sut = new RoleService(
                new Mock<IUnitOfWork>().Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(Guid.NewGuid(), new[] { 0 }, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("RoleId is invalid.");
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowKeyNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(userId, ct))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(DateTimeOffset.UtcNow).Object,
                CreateCurrentUserMock(Guid.NewGuid()).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(userId, new[] { 10 }, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowKeyNotFoundException_When_AnyRequestedRoleDoesNotExist()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            user.AssignRole(10, now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(20, ct))
                .ReturnsAsync((Role?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(user.Id, new[] { 20 }, ct);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Role not found.");

            user.UserRoles.Select(x => x.RoleId).Should().Equal(10);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceUserRolesAsync_Should_ThrowInvalidOperationException_When_AnyRequestedRoleIsInactive()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = CreateUser(now, actorUserId);
            var role = CreateRole(20, "support", "Support", now, actorUserId);
            role.Deactivate(now, actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock
                .Setup(x => x.FindByIdAsync(20, ct))
                .ReturnsAsync(role);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepositoryMock.Object);

            var sut = new RoleService(
                unitOfWorkMock.Object,
                CreateClockMock(now).Object,
                CreateCurrentUserMock(actorUserId).Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceUserRolesAsync(user.Id, new[] { 20 }, ct);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Role is not active.");

            user.UserRoles.Should().BeEmpty();
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static User CreateUser(DateTimeOffset now, Guid actorUserId)
        {
            return User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice Doe",
                "initial-password-hash",
                now,
                actorUserId);
        }

        private static Role CreateRole(int id, string key, string name, DateTimeOffset now, Guid actorUserId)
        {
            var role = Role.Create(key, name, now, actorUserId);
            SetId(role, id);
            return role;
        }

        private static void SetId(Role role, int id)
        {
            var property = typeof(Role).GetProperty(nameof(Role.Id));
            property.Should().NotBeNull();
            property!.SetValue(role, id);
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
