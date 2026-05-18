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
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Roles;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class RoleControllerTests
    {
        [Fact]
        public void RoleController_Should_HaveRouteApiRoles()
        {
            // Act
            var routeAttribute = typeof(RoleController)
                .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .OfType<RouteAttribute>()
                .Single();

            // Assert
            routeAttribute.Template.Should().Be("api/roles");
        }

        [Fact]
        public void List_Should_HaveHttpGetAttribute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.List));
            var httpGetAttribute = method!
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .OfType<HttpGetAttribute>()
                .SingleOrDefault();

            // Assert
            httpGetAttribute.Should().NotBeNull();
            httpGetAttribute!.Template.Should().BeNull();
        }

        [Fact]
        public void List_Should_HaveAuthorizeAttributeWithRoleReadPolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(RoleController.List));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:role.read");
        }

        [Fact]
        public async Task List_Should_DelegateToRoleServiceAndReturnOkResponse()
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

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.ListRolesAsync(ct))
                .ReturnsAsync(roles);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.List(ct);

            // Assert
            roleServiceMock.Verify(x => x.ListRolesAsync(ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<RoleResponse>>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().BeNull();
            payload.Data.Should().NotBeNull();
            payload.Data!.Should().HaveCount(2);
            payload.Data.Select(x => x.Name).Should().Equal("Admin", "Support");
            payload.Data.Should().OnlyContain(x => x.IsActive);
        }

        [Fact]
        public void Create_Should_HaveSingleHttpPostAttribute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.Create));
            var httpPostAttributes = method!
                .GetCustomAttributes(typeof(HttpPostAttribute), inherit: true)
                .OfType<HttpPostAttribute>()
                .ToArray();

            // Assert
            httpPostAttributes.Should().ContainSingle();
            httpPostAttributes.Single().Template.Should().BeNull();
        }

        [Fact]
        public void Create_Should_HaveAuthorizeAttributeWithRoleCreatePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(RoleController.Create));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:role.create");
        }

        [Fact]
        public async Task Create_Should_DelegateToRoleServiceAndReturnCreatedRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.CreateRoleAsync("Admin", ct))
                .ReturnsAsync(role);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Create(new CreateRoleRequest { Name = "Admin" }, ct);

            // Assert
            roleServiceMock.Verify(x => x.CreateRoleAsync("Admin", ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role created successfully.");
            payload.Data.Should().NotBeNull();
            payload.Data!.Name.Should().Be("Admin");
            payload.Data.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Create_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new RoleController(new Mock<IRoleService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.Create(null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void Update_Should_HaveHttpPutAttributeWithRoleIdRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.Update));
            var httpPutAttribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .OfType<HttpPutAttribute>()
                .SingleOrDefault();

            // Assert
            httpPutAttribute.Should().NotBeNull();
            httpPutAttribute!.Template.Should().Be("{roleId:int}");
        }

        [Fact]
        public void Update_Should_HaveAuthorizeAttributeWithRoleUpdatePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(RoleController.Update));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:role.update");
        }

        [Fact]
        public async Task Update_Should_DelegateToRoleServiceAndReturnUpdatedRole()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", now, actorUserId);
            role.Rename("Administrator", now.AddMinutes(1), actorUserId);

            var ct = new CancellationTokenSource().Token;

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.UpdateRoleAsync(12, "Administrator", ct))
                .ReturnsAsync(role);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Update(12, new UpdateRoleRequest { Name = "Administrator" }, ct);

            // Assert
            roleServiceMock.Verify(x => x.UpdateRoleAsync(12, "Administrator", ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role updated successfully.");
            payload.Data.Should().NotBeNull();
            payload.Data!.Name.Should().Be("Administrator");
            payload.Data.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Update_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new RoleController(new Mock<IRoleService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.Update(1, null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void Delete_Should_HaveHttpDeleteAttributeWithRoleIdRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.Delete));
            var httpDeleteAttribute = method!
                .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true)
                .OfType<HttpDeleteAttribute>()
                .SingleOrDefault();

            // Assert
            httpDeleteAttribute.Should().NotBeNull();
            httpDeleteAttribute!.Template.Should().Be("{roleId:int}");
        }

        [Fact]
        public void Delete_Should_HaveAuthorizeAttributeWithRoleDeletePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(RoleController.Delete));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:role.delete");
        }

        [Fact]
        public async Task Delete_Should_DelegateToRoleServiceAndReturnSuccessMessage()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.DeleteRoleAsync(12, ct))
                .Returns(Task.CompletedTask);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Delete(12, ct);

            // Assert
            roleServiceMock.Verify(x => x.DeleteRoleAsync(12, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role deleted successfully.");
            payload.Data.Should().BeNull();
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(string methodName)
        {
            var method = typeof(RoleController).GetMethod(methodName);

            var authorizeAttribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            authorizeAttribute.Should().NotBeNull();
            return authorizeAttribute!;
        }

        private static Mock<ICurrentUser> CreateCurrentUserMock()
        {
            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
            return currentUserMock;
        }
    }
}
