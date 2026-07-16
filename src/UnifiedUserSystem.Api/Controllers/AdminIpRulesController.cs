using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/admin/ip-rules")]
public sealed class AdminIpRulesController : AppControllerBase
{
    private readonly IIpManagementService _ipManagementService;

    public AdminIpRulesController(
        IIpManagementService ipManagementService,
        ICurrentUser currentUser)
        : base(currentUser)
    {
        _ipManagementService = ipManagementService;
    }

    [Authorize(Policy = OperationPolicyNames.IpRulesRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IpRuleResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpRuleResponse>>>> List(CancellationToken ct)
    {
        var rules = await _ipManagementService.ListRulesAsync(ct);
        return OkResponse(rules);
    }

    [Authorize(Policy = OperationPolicyNames.IpRulesCreate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IpRuleResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<IpRuleResponse>>> Create(
        [FromBody] CreateIpRuleRequest request,
        CancellationToken ct)
    {
        var rule = await _ipManagementService.CreateRuleAsync(request, ct);
        return CreatedResponse(nameof(List), null, rule);
    }

    [Authorize(Policy = OperationPolicyNames.IpRulesUpdate)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IpRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IpRuleResponse>>> Update(
        Guid id,
        [FromBody] UpdateIpRuleRequest request,
        CancellationToken ct)
    {
        var rule = await _ipManagementService.UpdateRuleAsync(id, request, ct);
        return OkResponse(rule);
    }

    [Authorize(Policy = OperationPolicyNames.IpRulesDisable)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Disable(Guid id, CancellationToken ct)
    {
        await _ipManagementService.DisableRuleAsync(id, ct);
        return OkMessage("IP rule disabled successfully.");
    }
}