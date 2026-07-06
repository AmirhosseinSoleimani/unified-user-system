using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces.Services;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.Contracts.DTOs.Permissions;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _perm;

        public PermissionController(IPermissionService perm)
        {
            _perm = perm;
        }

        [Authorize(Policy = OperationPolicyNames.PermissionsGrant)]
        [HttpPost("grant")]
        public async Task<IActionResult> Grant([FromBody] GrantPermissionRequest req)
        {
            await _perm.GrantOperationToRoleAsync(req.RoleId, req.OperationId);
            return NoContent();
        }


        [Authorize(Policy = OperationPolicyNames.PermissionsRevoke)]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokePermissionRequest req)
        {
            await _perm.RevokeOperationFromRoleAsync(req.RoleId, req.OperationId);
            return NoContent();
        }
    }
}
