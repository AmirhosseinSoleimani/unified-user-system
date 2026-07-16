using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
public class AppControllerBase : ControllerBase
{
    protected ICurrentUser CurrentUserService { get; }

    protected AppControllerBase(ICurrentUser currentUserService)
    {
        CurrentUserService = currentUserService;
    }

    protected string Localize(
        string code,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string? fallbackMessage = null)
    {
        var resolver = HttpContext.RequestServices.GetService<IRequestLocaleResolver>();
        var localizer = HttpContext.RequestServices.GetService<IBusinessMessageLocalizer>()
                        ?? DictionaryBusinessMessageLocalizer.Instance;

        var locale = resolver?.Resolve(HttpContext)
                     ?? ResolveFallbackLocale(
                         HttpContext.Request.Headers["Accept-Language"].ToString());

        return localizer.Get(code, locale, parameters, fallbackMessage);
    }

    protected ActionResult<ApiResponse<object>> OkMessage(string message) 
        => Ok(ApiResponse<object>.Ok(null, message));

    protected ActionResult<ApiResponse<object>> OkMessageCode(string code) 
        => Ok(ApiResponse<object>.Ok(null, Localize(code), code));

    protected ActionResult<ApiResponse<T>> OkResponse<T>(
        T data,
        string? message = null,
        string? code = null) 
        => Ok(ApiResponse<T>.Ok(data, message, code));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
        string actionName,
        object? routeValues,
        T data,
        string? message = null)
        => CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<object>> NoContentResponse()
        => StatusCode(StatusCodes.Status204NoContent);

    protected ActionResult<ApiResponse<object>> BadRequestResponse(
        string? message = null,
        object? errors = null,
        string code = MessageCodes.BadRequest)
        => BadRequest(Failure(code, message, errors));

    protected ActionResult<ApiResponse<object>> UnauthorizedResponse(
        string? message = null,
        string code = MessageCodes.Unauthorized)
        => StatusCode(StatusCodes.Status401Unauthorized, Failure(code, message));

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(
        string? message = null,
        string code = MessageCodes.Unauthorized)
        => StatusCode(
            StatusCodes.Status401Unauthorized,
            ApiResponse<T>.Fail(
                message ?? Localize(code),
                code: code,
                traceId: HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<object>> ForbiddenResponse(
        string? message = null,
        string code = MessageCodes.Forbidden)
        => StatusCode(StatusCodes.Status403Forbidden, Failure(code, message));

    protected ActionResult<ApiResponse<object>> NotFoundResponse(
        string? message = null,
        string code = MessageCodes.NotFound)
        => NotFound(Failure(code, message));

    protected ActionResult<ApiResponse<object>> ConflictResponse(
        string? message = null,
        object? errors = null,
        string code = MessageCodes.Conflict)
        => Conflict(Failure(code, message, errors));

    protected ActionResult<ApiResponse<object>> FailureResponse(
        string? message = null,
        int statusCode = StatusCodes.Status400BadRequest,
        object? errors = null,
        string code = MessageCodes.BadRequest)
        => StatusCode(statusCode, Failure(code, message, errors));

    private ApiResponse<object> Failure(string code, string? message, object? errors = null)
        => ApiResponse<object>.Fail(
            message ?? Localize(code),
            errors,
            code,
            HttpContext.TraceIdentifier);


    private static string ResolveFallbackLocale(string? acceptLanguage)
    => !string.IsNullOrWhiteSpace(acceptLanguage) &&
       acceptLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase)
        ? LocalizationOptions.EnglishLocale
        : LocalizationOptions.PersianLocale;

}
