using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class UsersControllerGetUserByIdTests
    {
        [Fact]
        public void GetUserById_Should_HaveHttpGetAttribute_WithGuidRouteTemplate()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.GetUserById));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .OfType<HttpGetAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{id:guid}");
        }

        [Fact]
        public void GetUserById_Should_HaveAuthorizeAttribute_WithUsersReadPolicy()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.GetUserById));
            var attribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Policy.Should().Be("OP:users.read");
        }

        [Fact]
        public async Task GetUserById_Should_DelegateToQueryService_AndReturnOkResponse_When_UserExists()
        {
            // Arrange
            var requestedId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;

            var response = new ProfileResponse
            {
                Id = requestedId,
                Email = "alice@example.com",
                Username = "alice",
                Fullname = "Alice Doe",
                IsActive = true,
                Roles = new[] { "Admin", "Support" }
            };

            var userQueryServiceMock = new Mock<IUserQueryService>();
            userQueryServiceMock
                .Setup(x => x.GetUserByIdAsync(requestedId, ct))
                .ReturnsAsync(response);

            var currentUserMock = new Mock<ICurrentUser>();
            var userCommandServiceMock = new Mock<IUserCommandService>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var sut = new UsersController(userQueryServiceMock.Object, userCommandServiceMock.Object, currentUserMock.Object);

            // Act
            var result = await sut.GetUserById(requestedId, ct);

            // Assert
            userQueryServiceMock.Verify(x => x.GetUserByIdAsync(requestedId, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<ProfileResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Data.Should().BeEquivalentTo(response);
            payload.Message.Should().BeNull();
        }
    }
}
