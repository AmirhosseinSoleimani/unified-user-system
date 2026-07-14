using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/app-metadata")]
public sealed class AppMetadataController : AppControllerBase
{
    private readonly IApplicationMetadataService _applicationMetadataService;

    public AppMetadataController(
        IApplicationMetadataService applicationMetadataService,
        ICurrentUser currentUser) : base(currentUser)
    {
        _applicationMetadataService = applicationMetadataService;
    }

    [HttpPost("bootstrap")]
    [ProducesResponseType(typeof(ApiResponse<AppBootstrapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AppBootstrapResponse>>> Bootstrap(
        [FromBody] AppBootstrapRequest request,
        CancellationToken ct)
    {
        var response = await _applicationMetadataService.GetBootstrapAsync(request, ct);
        return OkResponse(response);
    }
}
