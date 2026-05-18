using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Roles;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController : AppControllerBase
    {
        private readonly IRoleService _roles;

        public RoleController(IRoleService roles, ICurrentUser currentUser) : base (currentUser)
        {
            _roles = roles;
        }

        [Authorize(Policy = "OP:role.read")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleResponse>>>> List(CancellationToken ct)
        {
            var roles = await _roles.ListRolesAsync(ct);

            var response = roles
                .Select(ToResponse)
                .ToArray();

            return OkResponse<IReadOnlyList<RoleResponse>>(response);
        }

        [Authorize(Policy = "OP:role.read")]
        [HttpGet("{roleId:int}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> GetById([FromRoute] int roleId, CancellationToken ct)
        {
            var role = await _roles.GetRoleByIdAsync(roleId, ct)
                ?? throw new KeyNotFoundException("Role not found.");

            return OkResponse(ToResponse(role));
        }

        [Authorize(Policy = "OP:role.create")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> Create([FromBody] CreateRoleRequest req, CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var role = await _roles.CreateRoleAsync(req.Name, ct);

            return OkResponse(ToResponse(role), "Role created successfully.");
        }

        [Authorize(Policy = "OP:role.update")]
        [HttpPut("{roleId:int}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> Update(
            [FromRoute] int roleId,
            [FromBody] UpdateRoleRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var role = await _roles.UpdateRoleAsync(roleId, req.Name, ct);

            return OkResponse(ToResponse(role), "Role updated successfully.");
        }

        [Authorize(Policy = "OP:role.rename")]
        [HttpPut("{roleId:int}/rename")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<object>>> Rename(
            [FromRoute] int roleId,
            [FromBody] RenameRoleRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            await _roles.RenameRoleAsync(roleId, req.NewName, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:role.delete")]
        [HttpDelete("{roleId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int roleId, CancellationToken ct)
        {
            await _roles.DeleteRoleAsync(roleId, ct);

            return OkMessage("Role deleted successfully.");
        }

        [Authorize(Policy = "OP:role.activate")]
        [HttpPut("{roleId:int}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Activate([FromRoute] int roleId, CancellationToken ct)
        {
            await _roles.ActivateRoleAsync(roleId, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:role.deactivate")]
        [HttpPut("{roleId:int}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Deactivate([FromRoute] int roleId, CancellationToken ct)
        {
            await _roles.DeactivateRoleAsync(roleId, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:role.remove")]
        [HttpPost("remove")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Remove([FromBody] AssignRoleRequest req, CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            await _roles.RemoveRoleFromUserAsync(req.UserId, req.RoleId, ct);

            return NoContentResponse();
        }

        private static RoleResponse ToResponse(Role role)
        {
            if (role is null)
                throw new KeyNotFoundException("Role not found.");

            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                IsActive = role.IsActive
            };
        }
    }
}
