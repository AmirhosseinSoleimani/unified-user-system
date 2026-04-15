using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Roles;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController :ControllerBase
    {
        private readonly IRoleService _roles;

        public RoleController(IRoleService roles) 
        {
            _roles = roles;
        }

        [Authorize(Policy = "OP:role.create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest req)
        {
            var role = await _roles.CreateRoleAsync(req.Name);
            return Ok(new {role.Id, role.Name, role.IsActive});
        }

        [Authorize(Policy = "OP:role.rename")]
        [HttpPut("{roleId:int}/rename")]
        public async Task<IActionResult> Rename([FromRoute] int roleId, [FromBody] RenameRoleRequest req)
        {
            await _roles.RenameRoleAsync(roleId, req.NewName);
            return NoContent();
        }

        [Authorize(Policy = "OP:role.activate")]
        [HttpPut("{roleId:int}/activate")]
        public async Task<IActionResult> Activate([FromRoute] int roleId)
        {
            await _roles.ActivateRoleAsync(roleId);
            return NoContent();
        }

        [Authorize(Policy ="OP:role.deactivate")]
        [HttpPut("{roleId:int}/deactivate")]
        public async Task<IActionResult> Deactivate([FromRoute] int roleId)
        {
            await _roles.DeactivateRoleAsync(roleId);
            return NoContent();
        }

        [Authorize(Policy = "OP:role.remove")]
        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] AssignRoleRequest req)
        {
            await _roles.RemoveRoleFromUserAsync(req.UserId, req.RoleId);
            return NoContent();
        }
    }
}
