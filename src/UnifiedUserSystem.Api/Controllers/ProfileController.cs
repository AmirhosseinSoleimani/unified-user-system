using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : AppControllerBase
    {

        private readonly IProfileService _profileService;

        public ProfileController(
            IProfileService profileService,
            ICurrentUser currentUser) : base(currentUser) 
        { 
            _profileService = profileService;
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfileResponse>>> GetMyProfile(CancellationToken ct)
        {
            var profile = await _profileService.GetMyProfileAsync(ct);
            return OkResponse(profile);
        }
    }
}
