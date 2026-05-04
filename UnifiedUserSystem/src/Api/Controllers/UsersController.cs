using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Services.Identity;
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

        public UsersController(
            IUserQueryService userQueryService,
            ICurrentUser currentUser
            ) : base(currentUser)
        {
            _userQueryService = userQueryService;
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
    }
}
