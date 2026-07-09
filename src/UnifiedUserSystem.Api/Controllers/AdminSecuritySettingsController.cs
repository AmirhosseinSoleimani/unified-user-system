using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.Contracts.Common;

namespace UnifiedUserSystem.Api.Controllers;

[ApiController]
[Route("api/admin/security-settings")]
public sealed class AdminSecuritySettingsController : AppControllerBase
{
    private readonly ISecuritySettingsService _securitySettingsService;

    public AdminSecuritySettingsController(
        ISecuritySettingsService securitySettingsService,
        ICurrentUser currentUser)
        : base(currentUser)
    {
        _securitySettingsService = securitySettingsService;
    }

    [Authorize(Policy = OperationPolicyNames.SecuritySettingsRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SecuritySettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SecuritySettingsResponse>>> Get(CancellationToken ct)
    {
        var settings = await _securitySettingsService.GetEffectiveAsync(ct);
        return OkResponse(settings);
    }

    [Authorize(Policy = OperationPolicyNames.SecuritySettingsUpdate)]
    [EnableRateLimiting("SensitiveAdminRateLimit")]
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SecuritySettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SecuritySettingsResponse>>> Update(
        [FromBody] UpdateSecuritySettingsRequest request,
        CancellationToken ct)
    {
        var settings = await _securitySettingsService.UpdateAsync(request, ct);
        return OkResponse(settings, "Security settings updated successfully.");
    }
}
