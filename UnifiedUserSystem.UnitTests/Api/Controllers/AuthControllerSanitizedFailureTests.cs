using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Interfaces.Services;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class AuthControllerSanitizedFailureTests
    {
        [Theory]
        [InlineData("missing@example.com")]
        [InlineData("wrong-password@example.com")]
        [InlineData("locked@example.com")]
        [InlineData("cooldown@example.com")]
        public async Task Login_WithFailure_ShouldReturnGenericFailureMessage(string identifier)
        {
            var authService = new Mock<IAuthService>();
            var currentUser = new Mock<ICurrentUser>();

            authService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AuthResponse?)null);

            var sut = new AuthController(authService.Object, currentUser.Object);

            var result = await sut.Login(new LoginRequest
            {
                EmailOrUsername = identifier,
                Password = "Password123!"
            }, CancellationToken.None);

            var unauthorized = result.Result.Should().BeOfType<ObjectResult>().Subject;
            unauthorized.StatusCode.Should().Be(401);

            var payload = unauthorized.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
            payload.Success.Should().BeFalse();
            payload.Message.Should().Be("Authentication failed.");

            payload.Message.Should().NotBeNull();

            var message = payload.Message!.ToLowerInvariant();

            message.Should().NotContain("user not found");
            message.Should().NotContain("email not found");
            message.Should().NotContain("username not found");
            message.Should().NotContain("invalid password");
            message.Should().NotContain("account exists");
            message.Should().NotContain("locked email");
        }
    }
}