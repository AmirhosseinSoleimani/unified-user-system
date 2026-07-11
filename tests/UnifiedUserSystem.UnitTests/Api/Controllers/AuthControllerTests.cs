using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.UnitTests.Api.Controllers;

public class AuthControllerTests
{
    [Fact]
    public void AuthController_Should_HaveRouteApiAuth()
    {
        var routeAttribute = typeof(AuthController)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .OfType<RouteAttribute>()
            .Single();

        routeAttribute.Template.Should().Be("api/auth");
    }

    [Fact]
    public void Refresh_Should_HaveHttpPostRefreshAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Refresh));

        var attribute = method!
            .GetCustomAttributes(typeof(HttpPostAttribute), inherit: true)
            .OfType<HttpPostAttribute>()
            .Single();

        attribute.Template.Should().Be("refresh");
    }

    [Fact]
    public void Logout_Should_HaveAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Logout));

        var attribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        var authService = new Mock<IAuthService>();
        var currentUser = new Mock<ICurrentUser>();
        var request = new RefreshTokenRequest { RefreshToken = "invalid" };

        authService.Setup(x => x.RefreshAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        var sut = new AuthController(authService.Object, currentUser.Object);

        var result = await sut.Refresh(request, CancellationToken.None);

        var unauthorized = result.Result.Should().BeOfType<ObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
        unauthorized.Value.Should().BeOfType<ApiResponse<AuthResponse>>();
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        var currentUser = new Mock<ICurrentUser>();
        var request = new LogoutRequest { RefreshToken = "refresh-token" };

        currentUser.SetupGet(x => x.UserId).Returns(userId);
        authService.Setup(x => x.LogoutAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new AuthController(authService.Object, currentUser.Object);

        var result = await sut.Logout(request, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ApiResponse<object>>();
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        var currentUser = new Mock<ICurrentUser>();
        var request = new LogoutRequest { RefreshToken = "invalid" };

        currentUser.SetupGet(x => x.UserId).Returns(userId);
        authService.Setup(x => x.LogoutAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new AuthController(authService.Object, currentUser.Object);

        var result = await sut.Logout(request, CancellationToken.None);

        var unauthorized = result.Result.Should().BeOfType<ObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RevokeAllSessions_WithAuthenticatedUser_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser.SetupGet(x => x.UserId).Returns(userId);

        var sut = new AuthController(authService.Object, currentUser.Object);

        var result = await sut.RevokeAllSessions(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ApiResponse<object>>();
        authService.Verify(x => x.RevokeAllSessionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllSessions_WithoutAuthenticatedUser_ShouldReturnUnauthorized()
    {
        var authService = new Mock<IAuthService>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser.SetupGet(x => x.UserId).Returns((Guid?)null);

        var sut = new AuthController(authService.Object, currentUser.Object);

        var result = await sut.RevokeAllSessions(CancellationToken.None);

        var unauthorized = result.Result.Should().BeOfType<ObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }
}