using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.Common;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    public class AppControllerBase : ControllerBase
    {
        protected ICurrentUser CurrentUserService { get; }

        protected AppControllerBase(ICurrentUser currentUserService)
        {
            CurrentUserService = currentUserService;
        }
        protected ActionResult<ApiResponse<object>> OkMessage(string message)
        {
            return Ok(ApiResponse<object>.Ok(null, message));
        }

        protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        {
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
            string actionName,
            object? routeValues,
            T data,
            string? message = null
            )
        {
            return CreatedAtAction(
                actionName,
                routeValues,
                ApiResponse<T>.Ok(data, message));
        }

        protected ActionResult<ApiResponse<object>> NoContentResponse()
        {
            return StatusCode(StatusCodes.Status204NoContent);
        }

        protected ActionResult<ApiResponse<object>> BadRequestResponse(
            string message,
            object? errors = null)
        {
            return BadRequest(ApiResponse<object>.Fail(message, errors));
        }

        protected ActionResult<ApiResponse<object>> UnauthorizedResponse(
            string message = "Unauthorized.")
        {
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                ApiResponse<object>.Fail(message));
        }

        protected ActionResult<ApiResponse<object>> ForbiddenResponse(
            string message = "Forbidden.")
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(message));
        }

        protected ActionResult<ApiResponse<object>> NotFoundResponse(
            string message = "Resource not found.")
        {
            return NotFound(ApiResponse<object>.Fail(message));
        }

        protected ActionResult<ApiResponse<object>> ConflictResponse(
            string message,
            object? errors = null)
        {
            return Conflict(ApiResponse<object>.Fail(message, errors));
        }

        protected ActionResult<ApiResponse<object>> FailureResponse(
            string message,
            int statusCode = StatusCodes.Status400BadRequest,
            object? errors = null)
        {
            return StatusCode(statusCode, ApiResponse<object>.Fail(message, errors));
        }


    }
}
