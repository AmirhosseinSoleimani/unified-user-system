using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Contracts.DTOs;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) 
        { 
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req) 
        {
            try 
            {
                return Ok(await _auth.RegisterAsync(req));
            }
            catch (ArgumentException ex)
            { 
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                var res = await _auth.LoginAsync(req);
                if (res is null) return Unauthorized();
                return Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
