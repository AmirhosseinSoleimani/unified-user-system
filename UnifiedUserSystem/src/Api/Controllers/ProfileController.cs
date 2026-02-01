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
            var username = User.FindFirstValue("username");
            var role = User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();

            return Ok(new {sub , email, username, role });
        }
    }
}
