using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public void UsersController_Should_HaveRouteApiUsers()
        {
            // Act
            var routeAttribute = typeof(UsersController).GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .OfType<RouteAttribute>()
                .Single();

            // Assert
            routeAttribute.Template.Should().Be("api/users");
        }

        [Fact]
        public void GetActiveUsers_Should_HaveHttpGetAttribute()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.GetActiveUsers));
            var httpGetAttribute = method!
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .OfType<HttpGetAttribute>()
                .SingleOrDefault();

            // Assert
            httpGetAttribute.Should().NotBeNull();
        }

        [Fact]
        public void GetActiveUsers_Should_HaveAuthorizeAttributeWithUsersReadPolicy()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.GetActiveUsers));
            var authorizeAttribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            // Assert
            authorizeAttribute.Should().NotBeNull();
            authorizeAttribute!.Policy.Should().Be("OP:users.read");
        }

        [Fact]
        public async Task GetActiveUsers_Should_ReturnOkResponseWithUsers_When_ServiceReturnsUsers()
        {
            // Arrange
            var ct = new CancellationTokenSource().Token;

            var users = new List<ActiveUserListItemResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "alice@example.com",
                Username = "alice",
                Fullname = "Alice Doe",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "bob@example.com",
                Username = "bob",
                Fullname = "Bob Doe",
                IsActive = true
            }
        };

            var userQueryServiceMock = new Mock<IUserQueryService>();
            userQueryServiceMock
                .Setup(x => x.ListActiveUsersAsync(ct))
                .ReturnsAsync(users);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var sut = new UsersController(userQueryServiceMock.Object, currentUserMock.Object);

            // Act
            var result = await sut.GetActiveUsers(ct);

            // Assert
            userQueryServiceMock.Verify(x => x.ListActiveUsersAsync(ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<ActiveUserListItemResponse>>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Data.Should().BeEquivalentTo(users);
            payload.Message.Should().BeNull();
        }
    }
}
