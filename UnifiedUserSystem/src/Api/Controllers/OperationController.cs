using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Operations;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/operations")]
    public class OperationController : ControllerBase
    {
        private readonly IOperationService _ops;

        public OperationController(IOperationService ops) 
        {
            _ops = ops;
        }

        [Authorize(Policy = "OP:operation.create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOperationRequest req)
        {
            var op = await _ops.CreateOperationAsync(req.Key, req.Title);
            return Ok(new { op.Id, op.Key, op.Title, op.IsActive });
        }

        [Authorize(Policy = "OP:operation.renameTitle")]
        [HttpPut("{operationId:guid}/title")]
        public async Task<IActionResult> RenameTitle(Guid operationId, [FromBody] RenameOperationTitleRequest req)
        {
            await _ops.RenameOperationTitleAsync(operationId, req.NewTitle);
            return NoContent();
        }

        [Authorize(Policy = "OP:operation.changeKey")]
        [HttpPut("{operationId:guid}/key")]
        public async Task<IActionResult> ChangeKey(Guid operationId, [FromBody] ChangeOperationKeyRequest req)
        {
            await _ops.ChangeOperationKeyAsync(operationId, req.NewKey);
            return NoContent();
        }

        [Authorize(Policy = "OP:operation.activate")]
        [HttpPut("{operation:guid}/activate")]
        public async Task<IActionResult> Activate(Guid operationId)
        {
            await _ops.ActivateOperatioAsync(operationId);
            return NoContent();
        }

        [Authorize(Policy = "OP:operation.deactivate")]
        [HttpPut("{operationId:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid operationId)
        {
            await _ops.DeactivateOperationAsync(operationId);
            return NoContent();
        }
    }
}
