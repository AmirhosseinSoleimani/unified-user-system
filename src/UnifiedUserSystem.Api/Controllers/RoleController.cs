using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedUserSystem.src.Api.RateLimiting;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Security;
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

        [Authorize(Policy = OperationPolicyNames.RolesRead)]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesRead)]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesCreate)]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesUpdate)]
        [HttpPut("{roleId:int}")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesRename)]
        [HttpPut("{roleId:int}/rename")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesDelete)]
        [HttpDelete("{roleId:int}")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
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

        [Authorize(Policy = OperationPolicyNames.RolesActivate)]
        [HttpPut("{roleId:int}/activate")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Activate([FromRoute] int roleId, CancellationToken ct)
        {
            await _roles.ActivateRoleAsync(roleId, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = OperationPolicyNames.RolesDeactivate)]
        [HttpPut("{roleId:int}/deactivate")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Deactivate([FromRoute] int roleId, CancellationToken ct)
        {
            await _roles.DeactivateRoleAsync(roleId, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = OperationPolicyNames.RolesOperationsRead)]
        [HttpGet("{roleId:int}/operations")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<RoleOperationsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleOperationsResponse>>> GetOperations(
            [FromRoute] int roleId,
            CancellationToken ct)
        {
            var response = await _roles.GetRoleOperationsAsync(roleId, ct);

            return OkResponse(response);
        }

        [Authorize(Policy = OperationPolicyNames.RolesOperationsAssign)]
        [HttpPost("{roleId:int}/operations")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<RoleOperationsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoleOperationsResponse>>> AssignOperation(
            [FromRoute] int roleId,
            [FromBody] AssignRoleOperationRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var response = await _roles.AssignOperationToRoleAsync(roleId, req.OperationId, ct);

            return OkResponse(response, "Operation assigned successfully.");
        }

        [Authorize(Policy = OperationPolicyNames.RolesOperationsRemove)]
        [HttpDelete("{roleId:int}/operations/{operationId:guid}")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<RoleOperationsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleOperationsResponse>>> RemoveOperation(
            [FromRoute] int roleId,
            [FromRoute] Guid operationId,
            CancellationToken ct)
        {
            var response = await _roles.RemoveOperationFromRoleAsync(roleId, operationId, ct);

            return OkResponse(response, "Operation removed successfully.");
        }

        [Authorize(Policy = OperationPolicyNames.RolesOperationsReplace)]
        [HttpPut("{roleId:int}/operations")]
        [EnableRateLimiting("SensitiveAdminRateLimit")]
        [ProducesResponseType(typeof(ApiResponse<RoleOperationsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoleOperationsResponse>>> ReplaceOperations(
            [FromRoute] int roleId,
            [FromBody] ReplaceRoleOperationsRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var response = await _roles.ReplaceRoleOperationsAsync(roleId, req.OperationIds, ct);

            return OkResponse(response, "Role operations replaced successfully.");
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
