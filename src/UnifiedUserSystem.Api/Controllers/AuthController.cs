using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.RateLimiting;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : AppControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(
        IAuthService authService,
        ICurrentUser currentUser
        ) : base(currentUser)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest req,
        CancellationToken ct)
    {

        var response = await _authService.RegisterAsync(req, ct);
        AppendRefreshTokenCookie(
            response.RefreshToken,
            response.RefreshTokenExpiresAtUt
            );
        return OkResponse(response);
    }


    [HttpPost("login")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct)
    {
        if (req is null)
            throw new DomainException("Request is null.");

        var response = await _authService.LoginAsync(req, ct);

        if (response is null)
            return UnauthorizedResponse<LoginResponse>("Authentication failed.");

        return OkResponse(response);
    }

    [HttpPost("mfa/verify")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyMfa(
        [FromBody] VerifyMfaRequest req,
        CancellationToken ct)
    {
        if (req is null)
            throw new DomainException("Request is null.");

        var response = await _authService.VerifyMfaAsync(req, ct);

        if (response is null)
            return UnauthorizedResponse<AuthResponse>("MFA verification failed.");

        return OkResponse(response);
    }

    [HttpPost("refresh")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest req,
        CancellationToken ct)
    {

        var response = await _authService.RefreshAsync(req, ct);

        if (response is null)
            return UnauthorizedResponse<AuthResponse>("Refresh token is invalid.");

        return OkResponse(response);
    }


    [Authorize]
    [HttpPost("logout")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        [FromBody] LogoutRequest req,
        CancellationToken ct)
    {
        if (CurrentUserService.UserId is not Guid currentUserId || currentUserId == Guid.Empty)
            return UnauthorizedResponse();

        var loggedOut = await _authService.LogoutAsync(currentUserId, req, ct);

        if (!loggedOut)
            return UnauthorizedResponse("Refresh token is invalid.");

        return OkMessage("Logged out successfully.");
    }


    [Authorize]
    [HttpPost("revoke-all-sessions")]
    [EnableSecurityRateLimiting(SecurityRateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> RevokeAllSessions(
        CancellationToken ct)
    {
        if (CurrentUserService.UserId is not Guid currentUserId || currentUserId == Guid.Empty)
            return UnauthorizedResponse();

        await _authService.RevokeAllSessionsAsync(currentUserId, ct);
        return OkMessage("All sessions revoked successfully.");
    }
    private void AppendRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAtUtc)
    {
        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAtUtc,
                Path = "/api/auth"
            });
    }

}
