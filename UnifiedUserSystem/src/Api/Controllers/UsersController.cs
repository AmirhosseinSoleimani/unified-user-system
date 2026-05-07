using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : AppControllerBase
    {
        private readonly IUserQueryService _userQueryService;
        private readonly IUserCommandService _userCommandService;

        public UsersController(
            IUserQueryService userQueryService,
            IUserCommandService userCommandService,
            ICurrentUser currentUser
            ) : base(currentUser)
        {
            _userQueryService = userQueryService;
            _userCommandService = userCommandService;
        }

        [Authorize(Policy = "OP:users.read")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ActiveUserListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ActiveUserListItemResponse>>>> GetActiveUsers(CancellationToken ct)
        {
            var users = await _userQueryService.ListActiveUsersAsync(ct);
            return OkResponse(users);
        }

        [Authorize(Policy = "OP:users.read")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfileResponse>>> GetUserById([FromRoute] Guid id, CancellationToken ct)
        {
            var user = await _userQueryService.GetUserByIdAsync(id, ct);
            return OkResponse(user);
        }

        [Authorize(Policy = "OP:users.update")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfileResponse>>> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest req,
        CancellationToken ct)
        {
            var user = await _userCommandService.UpdateUserAsync(id, req, ct);
            return OkResponse(user);
        }

        [Authorize(Policy = "OP:users.deactivate")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateUser([FromRoute] Guid id, CancellationToken ct)
        {
            await _userCommandService.DeactivateUserAsync(id, ct);
            return OkMessage("User deactivated successfully.");
        }
    }
}
