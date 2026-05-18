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
using UnifiedUserSystem.src.Contracts.DTOs.Operations;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class OperationControllerTests
    {
        [Fact]
        public void OperationController_Should_HaveRouteApiOperations()
        {
            // Act
            var routeAttribute = typeof(OperationController)
                .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .OfType<RouteAttribute>()
                .Single();

            // Assert
            routeAttribute.Template.Should().Be("api/operations");
        }

        [Fact]
        public void List_Should_HaveHttpGetAttribute()
        {
            // Act
            var method = typeof(OperationController).GetMethod(nameof(OperationController.List));
            var httpGetAttribute = method!
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .OfType<HttpGetAttribute>()
                .SingleOrDefault();

            // Assert
            httpGetAttribute.Should().NotBeNull();
            httpGetAttribute!.Template.Should().BeNull();
        }

        [Fact]
        public void List_Should_HaveAuthorizeAttributeWithOperationReadPolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(OperationController.List));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:operation.read");
        }

        [Fact]
        public async Task List_Should_DelegateToOperationServiceAndReturnOkResponse()
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

            var operationServiceMock = new Mock<IOperationService>();
            operationServiceMock
                .Setup(x => x.ListOperationsAsync(ct))
                .ReturnsAsync(operations);

            var sut = new OperationController(operationServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.List(ct);

            // Assert
            operationServiceMock.Verify(x => x.ListOperationsAsync(ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<OperationResponse>>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().BeNull();
            payload.Data.Should().NotBeNull();
            payload.Data!.Should().HaveCount(2);
            payload.Data.Select(x => x.Key).Should().Equal("role.read", "role.create");
            payload.Data.Select(x => x.Title).Should().Equal("Read Roles", "Create Roles");
            payload.Data.Should().OnlyContain(x => x.IsActive);
        }

        [Fact]
        public void Create_Should_HaveHttpPostAttribute()
        {
            // Act
            var method = typeof(OperationController).GetMethod(nameof(OperationController.Create));
            var httpPostAttribute = method!
                .GetCustomAttributes(typeof(HttpPostAttribute), inherit: true)
                .OfType<HttpPostAttribute>()
                .SingleOrDefault();

            // Assert
            httpPostAttribute.Should().NotBeNull();
            httpPostAttribute!.Template.Should().BeNull();
        }

        [Fact]
        public void Create_Should_HaveAuthorizeAttributeWithOperationCreatePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(OperationController.Create));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:operation.create");
        }

        [Fact]
        public async Task Create_Should_DelegateToOperationServiceAndReturnCreatedOperation()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.create", "Create Roles", now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var operationServiceMock = new Mock<IOperationService>();
            operationServiceMock
                .Setup(x => x.CreateOperationAsync("role.create", "Create Roles", ct))
                .ReturnsAsync(operation);

            var sut = new OperationController(operationServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Create(new CreateOperationRequest
            {
                Key = "role.create",
                Title = "Create Roles"
            }, ct);

            // Assert
            operationServiceMock.Verify(x => x.CreateOperationAsync("role.create", "Create Roles", ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<OperationResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Operation created successfully.");
            payload.Data.Should().NotBeNull();
            payload.Data!.Id.Should().Be(operation.Id);
            payload.Data.Key.Should().Be("role.create");
            payload.Data.Title.Should().Be("Create Roles");
            payload.Data.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Create_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new OperationController(new Mock<IOperationService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.Create(null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void Update_Should_HaveHttpPutAttributeWithOperationIdRoute()
        {
            // Act
            var method = typeof(OperationController).GetMethod(nameof(OperationController.Update));
            var httpPutAttribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .OfType<HttpPutAttribute>()
                .SingleOrDefault();

            // Assert
            httpPutAttribute.Should().NotBeNull();
            httpPutAttribute!.Template.Should().Be("{operationId:guid}");
        }

        [Fact]
        public void Update_Should_HaveAuthorizeAttributeWithOperationUpdatePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(OperationController.Update));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:operation.update");
        }

        [Fact]
        public async Task Update_Should_DelegateToOperationServiceAndReturnUpdatedOperation()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var operation = Operation.Create("role.read", "Read Roles", now, actorUserId);
            operation.ChangeKey("role.update", now.AddMinutes(1), actorUserId);
            operation.RenameTitle("Update Roles", now.AddMinutes(1), actorUserId);

            var ct = new CancellationTokenSource().Token;

            var operationServiceMock = new Mock<IOperationService>();
            operationServiceMock
                .Setup(x => x.UpdateOperationAsync(operation.Id, "role.update", "Update Roles", ct))
                .ReturnsAsync(operation);

            var sut = new OperationController(operationServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Update(operation.Id, new UpdateOperationRequest
            {
                Key = "role.update",
                Title = "Update Roles"
            }, ct);

            // Assert
            operationServiceMock.Verify(x => x.UpdateOperationAsync(operation.Id, "role.update", "Update Roles", ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<OperationResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Operation updated successfully.");
            payload.Data.Should().NotBeNull();
            payload.Data!.Id.Should().Be(operation.Id);
            payload.Data.Key.Should().Be("role.update");
            payload.Data.Title.Should().Be("update roles");
            payload.Data.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Update_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var sut = new OperationController(new Mock<IOperationService>().Object, CreateCurrentUserMock().Object);

            // Act
            Func<Task> act = async () => await sut.Update(Guid.NewGuid(), null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Request is null.");
        }

        [Fact]
        public void Delete_Should_HaveHttpDeleteAttributeWithOperationIdRoute()
        {
            // Act
            var method = typeof(OperationController).GetMethod(nameof(OperationController.Delete));
            var httpDeleteAttribute = method!
                .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true)
                .OfType<HttpDeleteAttribute>()
                .SingleOrDefault();

            // Assert
            httpDeleteAttribute.Should().NotBeNull();
            httpDeleteAttribute!.Template.Should().Be("{operationId:guid}");
        }

        [Fact]
        public void Delete_Should_HaveAuthorizeAttributeWithOperationDeletePolicy()
        {
            // Act
            var authorizeAttribute = GetAuthorizeAttribute(nameof(OperationController.Delete));

            // Assert
            authorizeAttribute.Policy.Should().Be("OP:operation.delete");
        }

        [Fact]
        public async Task Delete_Should_DelegateToOperationServiceAndReturnSuccessMessage()
        {
            // Arrange
            var operationId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var operationServiceMock = new Mock<IOperationService>();
            operationServiceMock
                .Setup(x => x.DeleteOperationAsync(operationId, ct))
                .Returns(Task.CompletedTask);

            var sut = new OperationController(operationServiceMock.Object, CreateCurrentUserMock().Object);

            // Act
            var result = await sut.Delete(operationId, ct);

            // Assert
            operationServiceMock.Verify(x => x.DeleteOperationAsync(operationId, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Message.Should().Be("Operation deleted successfully.");
            payload.Data.Should().BeNull();
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(string methodName)
        {
            var method = typeof(OperationController).GetMethod(methodName);

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
