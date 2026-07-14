using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Localization;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/admin/error-messages")]

public sealed class ErrorMessagesController : AppControllerBase
{
    private readonly IErrorMessageService _errorMessageService;

    public ErrorMessagesController(
        IErrorMessageService errorMessageService,
        ICurrentUser currentUser) : base(currentUser)
    {
        _errorMessageService = errorMessageService;
    }

    [Authorize(Policy = OperationPolicyNames.ErrorMessagesRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ErrorMessageResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ErrorMessageResponse>>>> List(CancellationToken ct)
    {
        var messages = await _errorMessageService.ListAsync(ct);
        return OkResponse(messages);
    }

    [Authorize(Policy = OperationPolicyNames.ErrorMessagesUpdate)]
    [HttpPut("{key}")]
    [ProducesResponseType(typeof(ApiResponse<ErrorMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ErrorMessageResponse>>> Upsert(
        [FromRoute] string key,
        [FromBody] UpsertErrorMessageRequest request,
        CancellationToken ct)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        request.Key = key;

        var response = await _errorMessageService.UpsertAsync(request, ct);
        return OkResponse(response, "Error message saved successfully.");
    }

    [Authorize(Policy = OperationPolicyNames.ErrorMessagesUpdate)]
    [HttpPost("refresh-cache")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<object>>> RefreshCache(CancellationToken ct)
    {
        await _errorMessageService.RefreshCacheAsync(ct);
        return OkMessage("Error message cache refreshed successfully.");
    }
}
