using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserQueryServiceGetUserByIdTests
    {
        [Fact]
        public async Task GetUserByIdAsync_Should_ReturnMappedSafeResponse_When_UserExists()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();

            var user = User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice Doe",
                "hashed-password",
                now,
                actorUserId);

            var adminRole = Role.Create("admin", "Admin", now, actorUserId);
            var supportRole = Role.Create("support", "Support", now, actorUserId);

            user.AssignRole(1, now, actorUserId);
            user.AssignRole(2, now, actorUserId);

            var userRoles = user.UserRoles.ToArray();
            SetRole(userRoles[0], adminRole);
            SetRole(userRoles[1], supportRole);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.Users)
                .Returns(userRepositoryMock.Object);

            var sut = new UserQueryService(unitOfWorkMock.Object);

            // Act
            var result = await sut.GetUserByIdAsync(user.Id, CancellationToken.None);

            // Assert
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Username.Should().Be(user.Username);
            result.Fullname.Should().Be(user.Fullname);
            result.IsActive.Should().Be(user.IsActive);
            result.Roles.Should().BeEquivalentTo(new[] { "Admin", "Support" });

            typeof(UnifiedUserSystem.src.Contracts.DTOs.Profile.ProfileResponse)
                .GetProperties()
                .Select(x => x.Name)
                .Should()
                .NotContain(new[] { "Password", "PasswordHash" });
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_ThrowKeyNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var requestedId = Guid.NewGuid();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(requestedId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.Users)
                .Returns(userRepositoryMock.Object);

            var sut = new UserQueryService(unitOfWorkMock.Object);

            // Act
            Func<Task> act = async () => await sut.GetUserByIdAsync(requestedId, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_CallFindByIdWithRolesAsync_WithRequestedId()
        {
            // Arrange
            var requestedId = Guid.NewGuid();
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var user = User.CreateNew(
                "bob@example.com",
                "bob",
                "Bob Doe",
                "hashed-password",
                now,
                actorUserId);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(requestedId, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.Users)
                .Returns(userRepositoryMock.Object);

            var sut = new UserQueryService(unitOfWorkMock.Object);

            // Act
            await sut.GetUserByIdAsync(requestedId, ct);

            // Assert
            userRepositoryMock.Verify(x => x.FindByIdWithRolesAsync(requestedId, ct), Times.Once);
        }

        private static void SetRole(UserRole userRole, Role role)
        {
            var property = typeof(UserRole).GetProperty(nameof(UserRole.Role), BindingFlags.Instance | BindingFlags.Public);
            var setter = property!.GetSetMethod(nonPublic: true);

            setter.Should().NotBeNull("UserRole.Role has a private setter that the test uses to build the exact current entity graph");
            setter!.Invoke(userRole, new object[] { role });
        }
    }
}
