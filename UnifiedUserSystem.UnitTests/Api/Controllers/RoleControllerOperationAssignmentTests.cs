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

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class RoleControllerOperationAssignmentTests
    {
        [Fact]
        public void GetOperations_Should_HaveHttpGetAttributeWithRoleOperationsRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.GetOperations));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .OfType<HttpGetAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{roleId:int}/operations");
        }

        [Fact]
        public void GetOperations_Should_HaveAuthorizeAttributeWithRolesOperationsReadPolicy()
        {
            // Act
            var attribute = GetAuthorizeAttribute(nameof(RoleController.GetOperations));

            // Assert
            attribute.Policy.Should().Be("OP:roles.operations.read");
        }

        [Fact]
        public async Task GetOperations_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;
            var operationId = Guid.NewGuid();

            var response = new RoleOperationsResponse
            {
                RoleId = 10,
                Operations = new[]
                {
                new RoleOperationItemResponse
                {
                    OperationId = operationId,
                    Key = "users.read",
                    Title = "Read Users",
                    IsActive = true
                }
            }
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.GetRoleOperationsAsync(10, ct))
                .ReturnsAsync(response);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.GetOperations(10, ct);

            // Assert
            roleServiceMock.Verify(x => x.GetRoleOperationsAsync(10, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleOperationsResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().BeNull();
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void AssignOperation_Should_HaveHttpPostAttributeWithRoleOperationsRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.AssignOperation));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpPostAttribute), inherit: true)
                .OfType<HttpPostAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{roleId:int}/operations");
        }

        [Fact]
        public void AssignOperation_Should_HaveAuthorizeAttributeWithRolesOperationsAssignPolicy()
        {
            // Act
            var attribute = GetAuthorizeAttribute(nameof(RoleController.AssignOperation));

            // Assert
            attribute.Policy.Should().Be("OP:roles.operations.assign");
        }

        [Fact]
        public async Task AssignOperation_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;
            var operationId = Guid.NewGuid();

            var response = new RoleOperationsResponse
            {
                RoleId = 10,
                Operations = new[]
                {
                new RoleOperationItemResponse
                {
                    OperationId = operationId,
                    Key = "users.read",
                    Title = "Read Users",
                    IsActive = true
                }
            }
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.AssignOperationToRoleAsync(10, operationId, ct))
                .ReturnsAsync(response);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.AssignOperation(10, new AssignRoleOperationRequest { OperationId = operationId }, ct);

            // Assert
            roleServiceMock.Verify(x => x.AssignOperationToRoleAsync(10, operationId, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleOperationsResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Operation assigned successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task AssignOperation_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new RoleController(new Mock<IRoleService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.AssignOperation(10, null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void RemoveOperation_Should_HaveHttpDeleteAttributeWithRoleOperationRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.RemoveOperation));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true)
                .OfType<HttpDeleteAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{roleId:int}/operations/{operationId:guid}");
        }

        [Fact]
        public void RemoveOperation_Should_HaveAuthorizeAttributeWithRolesOperationsRemovePolicy()
        {
            // Act
            var attribute = GetAuthorizeAttribute(nameof(RoleController.RemoveOperation));

            // Assert
            attribute.Policy.Should().Be("OP:roles.operations.remove");
        }

        [Fact]
        public async Task RemoveOperation_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;
            var operationId = Guid.NewGuid();

            var response = new RoleOperationsResponse
            {
                RoleId = 10,
                Operations = Array.Empty<RoleOperationItemResponse>()
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.RemoveOperationFromRoleAsync(10, operationId, ct))
                .ReturnsAsync(response);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.RemoveOperation(10, operationId, ct);

            // Assert
            roleServiceMock.Verify(x => x.RemoveOperationFromRoleAsync(10, operationId, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleOperationsResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Operation removed successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void ReplaceOperations_Should_HaveHttpPutAttributeWithRoleOperationsRoute()
        {
            // Act
            var method = typeof(RoleController).GetMethod(nameof(RoleController.ReplaceOperations));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .OfType<HttpPutAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{roleId:int}/operations");
        }

        [Fact]
        public void ReplaceOperations_Should_HaveAuthorizeAttributeWithRolesOperationsReplacePolicy()
        {
            // Act
            var attribute = GetAuthorizeAttribute(nameof(RoleController.ReplaceOperations));

            // Assert
            attribute.Policy.Should().Be("OP:roles.operations.replace");
        }

        [Fact]
        public async Task ReplaceOperations_Should_DelegateToRoleServiceAndReturnOkResponse()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;
            var operationIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var response = new RoleOperationsResponse
            {
                RoleId = 10,
                Operations = new[]
                {
                new RoleOperationItemResponse
                {
                    OperationId = operationIds[0],
                    Key = "users.read",
                    Title = "Read Users",
                    IsActive = true
                },
                new RoleOperationItemResponse
                {
                    OperationId = operationIds[1],
                    Key = "users.update",
                    Title = "Update Users",
                    IsActive = true
                }
            }
            };

            var roleServiceMock = new Mock<IRoleService>();
            roleServiceMock
                .Setup(x => x.ReplaceRoleOperationsAsync(10, operationIds, ct))
                .ReturnsAsync(response);

            var sut = new RoleController(roleServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.ReplaceOperations(10, new ReplaceRoleOperationsRequest { OperationIds = operationIds }, ct);

            // Assert
            roleServiceMock.Verify(x => x.ReplaceRoleOperationsAsync(10, operationIds, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<RoleOperationsResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Role operations replaced successfully.");
            payload.Data.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ReplaceOperations_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new RoleController(new Mock<IRoleService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.ReplaceOperations(10, null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(string methodName)
        {
            var method = typeof(RoleController).GetMethod(methodName);

            var attribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            attribute.Should().NotBeNull();
            return attribute!;
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
