using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Operations;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/operations")]
    public class OperationController : AppControllerBase
    {
        private readonly IOperationService _ops;

        public OperationController(IOperationService ops, ICurrentUser currentUser) : base(currentUser) 
        {
            _ops = ops;
        }

        [Authorize(Policy = "OP:operation.read")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OperationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OperationResponse>>>> List(CancellationToken ct)
        {
            var operations = await _ops.ListOperationsAsync(ct);

            var response = operations
                .Select(ToResponse)
                .ToArray();

            return OkResponse<IReadOnlyList<OperationResponse>>(response);
        }

        [Authorize(Policy = "OP:operation.read")]
        [HttpGet("{operationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OperationResponse>>> GetById(
            [FromRoute] Guid operationId,
            CancellationToken ct)
        {
            var operation = await _ops.GetOperationByIdAsync(operationId, ct)
                ?? throw new KeyNotFoundException("Operation not found.");

            return OkResponse(ToResponse(operation));
        }

        [Authorize(Policy = "OP:operation.create")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<OperationResponse>>> Create(
            [FromBody] CreateOperationRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var op = await _ops.CreateOperationAsync(req.Key, req.Title, ct);

            return OkResponse(ToResponse(op), "Operation created successfully.");
        }

        [Authorize(Policy = "OP:operation.update")]
        [HttpPut("{operationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<OperationResponse>>> Update(
            [FromRoute] Guid operationId,
            [FromBody] UpdateOperationRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var op = await _ops.UpdateOperationAsync(operationId, req.Key, req.Title, ct);

            return OkResponse(ToResponse(op), "Operation updated successfully.");
        }

        [Authorize(Policy = "OP:operation.renameTitle")]
        [HttpPut("{operationId:guid}/title")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RenameTitle(
            [FromRoute] Guid operationId,
            [FromBody] RenameOperationTitleRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            await _ops.RenameOperationTitleAsync(operationId, req.NewTitle, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:operation.changeKey")]
        [HttpPut("{operationId:guid}/key")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<object>>> ChangeKey(
            [FromRoute] Guid operationId,
            [FromBody] ChangeOperationKeyRequest req,
            CancellationToken ct)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            await _ops.ChangeOperationKeyAsync(operationId, req.NewKey, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:operation.delete")]
        [HttpDelete("{operationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(
            [FromRoute] Guid operationId,
            CancellationToken ct)
        {
            await _ops.DeleteOperationAsync(operationId, ct);

            return OkMessage("Operation deleted successfully.");
        }

        [Authorize(Policy = "OP:operation.activate")]
        [HttpPut("{operationId:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Activate(
            [FromRoute] Guid operationId,
            CancellationToken ct)
        {
            await _ops.ActivateOperationAsync(operationId, ct);

            return NoContentResponse();
        }

        [Authorize(Policy = "OP:operation.deactivate")]
        [HttpPut("{operationId:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> Deactivate(
            [FromRoute] Guid operationId,
            CancellationToken ct)
        {
            await _ops.DeactivateOperationAsync(operationId, ct);

            return NoContentResponse();
        }

        private static OperationResponse ToResponse(Operation operation)
        {
            return new OperationResponse
            {
                Id = operation.Id,
                Key = operation.Key,
                Title = operation.Title,
                IsActive = operation.IsActive
            };
        }
    }
}
