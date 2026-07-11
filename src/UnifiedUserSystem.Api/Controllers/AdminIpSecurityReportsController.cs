using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/admin/security-reports/ip")]
public sealed class AdminIpSecurityReportsController : AppControllerBase
{
    private readonly IIpSecurityReportService _reportService;

    public AdminIpSecurityReportsController(
        IIpSecurityReportService reportService,
        ICurrentUser currentUser)
        : base(currentUser)
    {
        _reportService = reportService;
    }

    [Authorize(Policy = OperationPolicyNames.IpSecurityReportsRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IpSecurityEventResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpSecurityEventResponse>>>> Search(
        [FromQuery] IpSecurityReportQuery query,
        CancellationToken ct)
    {
        var events = await _reportService.SearchAsync(query, ct);
        return OkResponse(events);
    }
}
