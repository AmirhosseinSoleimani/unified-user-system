using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class UsersControllerUpdateUserTests
    {
        [Fact]
        public void UpdateUser_Should_HaveHttpPutAttribute_WithGuidRouteTemplate()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.UpdateUser));
            var attribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .OfType<HttpPutAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Template.Should().Be("{id:guid}");
        }

        [Fact]
        public void UpdateUser_Should_HaveAuthorizeAttribute_WithUsersUpdatePolicy()
        {
            // Act
            var method = typeof(UsersController).GetMethod(nameof(UsersController.UpdateUser));
            var attribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .SingleOrDefault();

            // Assert
            attribute.Should().NotBeNull();
            attribute!.Policy.Should().Be("OP:users.update");
        }

        [Fact]
        public async Task UpdateUser_Should_DelegateToCommandService_AndReturnOkResponse_When_UpdateSucceeds()
        {
            // Arrange
            var requestedId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            var req = new UpdateUserRequest
            {
                Fullname = "Updated Name"
            };

            var response = new ProfileResponse
            {
                Id = requestedId,
                Email = "alice@example.com",
                Username = "alice",
                Fullname = "Updated Name",
                IsActive = true,
                Roles = Array.Empty<string>()
            };

            var userQueryServiceMock = new Mock<IUserQueryService>();
            var userCommandServiceMock = new Mock<IUserCommandService>();
            userCommandServiceMock
                .Setup(x => x.UpdateUserAsync(requestedId, req, ct))
                .ReturnsAsync(response);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var sut = new UsersController(
                userQueryServiceMock.Object,
                userCommandServiceMock.Object,
                currentUserMock.Object);

            // Act
            var result = await sut.UpdateUser(requestedId, req, ct);

            // Assert
            userCommandServiceMock.Verify(x => x.UpdateUserAsync(requestedId, req, ct), Times.Once);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<ApiResponse<ProfileResponse>>().Subject;

            payload.Success.Should().BeTrue();
            payload.Data.Should().BeEquivalentTo(response);
            payload.Message.Should().BeNull();
        }
    }
}
