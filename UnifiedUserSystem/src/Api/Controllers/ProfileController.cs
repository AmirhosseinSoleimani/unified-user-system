using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new {sub , email, role });
        }
    }
}
