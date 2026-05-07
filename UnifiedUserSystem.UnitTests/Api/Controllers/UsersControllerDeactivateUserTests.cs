using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class UsersControllerDeactivateUserTests
    {
        [Fact]
        public void DeactivateUser_Should_HaveHttpDeleteAttribute_WithGuidRouteTemplate()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.DeactivateUser));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true)
                .OfType<HttpDeleteAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{id:guid}");
        }

        [Fact]
        public void DeactivateUser_Should_HaveAuthorizeAttribute_WithUsersDeactivatePolicy()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.DeactivateUser));
            var attribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Policy.Should().Be("OP:users.deactivate");
        }

        [Fact]
        public async Task DeactivateUser_Should_DelegateToCommandService_When_Called()
        {
            // Arrange
            var requestedId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var userQueryServiceMock = new Mock<IUserQueryService>();
            var userCommandServiceMock = new Mock<IUserCommandService>();
            userCommandServiceMock
                .Setup(x => x.DeactivateUserAsync(requestedId, ct))
                .Returns(Task.CompletedTask);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var sut = new UsersController(
                userQueryServiceMock.Object,
                userCommandServiceMock.Object,
                currentUserMock.Object);

            // Act
            var result = await sut.DeactivateUser(requestedId, ct);

            // Assert
            userCommandServiceMock.Verify(x => x.DeactivateUserAsync(requestedId, ct), Times.Once);
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
