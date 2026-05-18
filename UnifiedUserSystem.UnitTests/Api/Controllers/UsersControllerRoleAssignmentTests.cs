using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class UsersControllerRoleAssignmentTests
    {
        [Fact]
        public void AssignRole_Should_HaveHttpPostAttributeWithUserRolesRoute()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.AssignRole));
            var httpPostAttribute = method!
                .GetCustomAttributes(typeof(HttpPostAttribute), inherit: true)
                .OfType<HttpPostAttribute>()
                .SingleOrDefault();

            // Assert
            httpPostAttribute.Should().NotBeNull();
            httpPostAttribute!.Template.Should().Be("{userId:guid}/roles");
        }

        [Fact]
        public void AssignRole_Should_HaveAuthorizeAttributeWithUsersRolesAssignPolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(UsersController.AssignRole));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:users.roles.assign");
        }

        [Fact]
        public async Task AssignRole_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var response = new UserRolesResponse
            {
                UserId = userId,
                Roles = new[]
                {
                new UserRoleItemResponse
                {
                    RoleId = 10,
                    Name = "Admin",
                    IsActive = true
                }
            }
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.AssignRoleToUserAsync(userId, 10, ct))
                .ReturnsAsync(response);

            var sut = CreateController(roleServiceMock.Object);

            // Act
            var result = await sut.AssignRole(userId, new AssignUserRoleRequest { RoleId = 10 }, ct);

            // Assert
            roleServiceMock.Verify(x => x.AssignRoleToUserAsync(userId, 10, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<UserRolesResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role assigned successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task AssignRole_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = CreateController();

            // Act
            Func<Task> act = async () => await sut.AssignRole(Guid.NewGuid(), null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void RemoveRole_Should_HaveHttpDeleteAttributeWithUserRoleRoute()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.RemoveRole));
            var httpDeleteAttribute = method!
                .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true)
                .OfType<HttpDeleteAttribute>()
                .SingleOrDefault();

            // Assert
            httpDeleteAttribute.Should().NotBeNull();
            httpDeleteAttribute!.Template.Should().Be("{userId:guid}/roles/{roleId:int}");
        }

        [Fact]
        public void RemoveRole_Should_HaveAuthorizeAttributeWithUsersRolesRemovePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(UsersController.RemoveRole));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:users.roles.remove");
        }

        [Fact]
        public async Task RemoveRole_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var response = new UserRolesResponse
            {
                UserId = userId,
                Roles = Array.Empty<UserRoleItemResponse>()
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.RemoveRoleFromUserAsync(userId, 10, ct))
                .ReturnsAsync(response);

            var sut = CreateController(roleServiceMock.Object);

            // Act
            var result = await sut.RemoveRole(userId, 10, ct);

            // Assert
            roleServiceMock.Verify(x => x.RemoveRoleFromUserAsync(userId, 10, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<UserRolesResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role removed successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void ReplaceRoles_Should_HaveHttpPutAttributeWithUserRolesRoute()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.ReplaceRoles));
            var httpPutAttribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .OfType<HttpPutAttribute>()
                .SingleOrDefault();

            // Assert
            httpPutAttribute.Should().NotBeNull();
            httpPutAttribute!.Template.Should().Be("{userId:guid}/roles");
        }

        [Fact]
        public void ReplaceRoles_Should_HaveAuthorizeAttributeWithUsersRolesReplacePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(UsersController.ReplaceRoles));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:users.roles.replace");
        }

        [Fact]
        public async Task ReplaceRoles_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            var roleIds = new[] { 10, 20 };

            var response = new UserRolesResponse
            {
                UserId = userId,
                Roles = new[]
                {
                new UserRoleItemResponse
                {
                    RoleId = 10,
                    Name = "Admin",
                    IsActive = true
                },
                new UserRoleItemResponse
                {
                    RoleId = 20,
                    Name = "Support",
                    IsActive = true
                }
            }
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.ReplaceUserRolesAsync(userId, roleIds, ct))
                .ReturnsAsync(response);

            var sut = CreateController(roleServiceMock.Object);

            // Act
            var result = await sut.ReplaceRoles(userId, new ReplaceUserRolesRequest { RoleIds = roleIds }, ct);

            // Assert
            roleServiceMock.Verify(x => x.ReplaceUserRolesAsync(userId, roleIds, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<UserRolesResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("User roles replaced successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ReplaceRoles_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = CreateController();

            // Act
            Func<Task> act = async () => await sut.ReplaceRoles(Guid.NewGuid(), null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(string methodName)
        {
            var method = typeof(UsersController).GetMethod(methodName);

            var authorizeAttribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            authorizeAttribute.Should().NotBeNull();
            return authorizeAttribute!;
        }

        private static UsersController CreateController(IRoleService? roleService = null)
        {
            var userQueryServiceMock = new Mock<IUserQueryService>();
            var userCommandServiceMock = new Mock<IUserCommandService>();
            var currentUserMock = new Mock<ICurrentUser>();

            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            return new UsersController(
                userQueryServiceMock.Object,
                userCommandServiceMock.Object,
                roleService ?? new Mock<IRoleService>().Object,
                currentUserMock.Object);
        }
    }
}
