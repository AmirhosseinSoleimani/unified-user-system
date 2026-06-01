using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : AppControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(
            IAuthService authService,
            ICurrentUser currentUser
            ) : base( currentUser )
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
            [FromBody] RegisterRequest req,
            CancellationToken ct)
        {

            var response = await _authService.RegisterAsync(req, ct);

            return OkResponse(response);
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
            [FromBody] LoginRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var response = await _authService.LoginAsync(req, ct);

            if (response is null)
                return UnauthorizedResponse<AuthResponse>("Invalid email/username or password.");

            return OkResponse(response);
        }


        [HttpPost("refresh")]
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Logout(
            [FromBody] LogoutRequest req,
            CancellationToken ct)
        {
            await _authService.LogoutAsync(req, ct);
            return OkMessage("Logged out successfully.");
        }


        [Authorize]
        [HttpPost("revoke-all-sessions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> RevokeAllSessions(
            [FromBody] RevokeAllSessionsRequest req,
            CancellationToken ct)
        {
            if (CurrentUserService.UserId is not Guid currentUserId || currentUserId == Guid.Empty)
                return UnauthorizedResponse();

            await _authService.RevokeAllSessionsAsync(currentUserId, ct);
            return OkMessage("All sessions revoked successfully.");
        }
    }
}
