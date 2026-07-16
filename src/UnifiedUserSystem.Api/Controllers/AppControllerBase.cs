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
        => Ok(ApiResponse<object>.Ok(new { }));

    protected ActionResult<ApiResponse<T>> OkResponse<T>(
        T data
        ) 
        => Ok(ApiResponse<T>.Ok(data));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
        string actionName,
        object? routeValues,
        T data
        )
        => CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data));

    protected ActionResult<ApiResponse<object>> NoContentResponse()
        => Ok(ApiResponse<object>.Ok(new { }));

    protected ActionResult<ApiResponse<object>> BadRequestResponse(
        string? message = null,
        string code = MessageCodes.BadRequest)
        => BadRequest(Failure(
            title: Localize(MessageCodes.BadRequest),
            description: message ?? Localize(code),
            resultCode: ApiResultCodes.BusinessError));

    protected ActionResult<ApiResponse<object>> UnauthorizedResponse(
        string? message = null,
        string code = MessageCodes.Unauthorized)
        => StatusCode(
            StatusCodes.Status401Unauthorized,
            Failure(
                title: Localize(MessageCodes.Unauthorized),
                description: message ?? Localize(code),
                resultCode: ApiResultCodes.AccessDenied));

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(
        string? message = null,
        string code = MessageCodes.Unauthorized)
        => StatusCode(
            StatusCodes.Status401Unauthorized,
            ApiResponse<T>.Fail(
               title: Localize(MessageCodes.Unauthorized),
               description: message ?? Localize(code),
               resultCode: ApiResultCodes.AccessDenied,
               traceId: HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<object>> ForbiddenResponse(
        string? message = null,
        string code = MessageCodes.Forbidden)
        => StatusCode(
            StatusCodes.Status403Forbidden,
            Failure(
                title: Localize(MessageCodes.Forbidden),
                description: message ?? Localize(code),
                resultCode: ApiResultCodes.AccessDenied));

    protected ActionResult<ApiResponse<object>> NotFoundResponse(
        string? message = null,
        string code = MessageCodes.NotFound)
        => NotFound(Failure(
            title: Localize(MessageCodes.NotFound),
            description: message ?? Localize(code),
            resultCode: ApiResultCodes.BusinessError));

    protected ActionResult<ApiResponse<object>> ConflictResponse(
        string? message = null,
        string code = MessageCodes.Conflict)
        => Conflict(Failure(
            title: Localize(MessageCodes.Conflict),
            description: message ?? Localize(code),
            resultCode: ApiResultCodes.BusinessError));

    protected ActionResult<ApiResponse<object>> FailureResponse(
        string? message = null,
        int statusCode = StatusCodes.Status400BadRequest,
        object? errors = null,
        string code = MessageCodes.BadRequest)
        => StatusCode(
            statusCode,
            Failure(
                title: Localize(code),
                description: message ?? Localize(code),
                resultCode: ResolveResultCode(statusCode)
            ));

    private ApiResponse<object> Failure(
        string title,
        string description,
        int resultCode)
        => ApiResponse<object>.Fail(
            title,
            description,
            resultCode,
            HttpContext.TraceIdentifier);

    private static int ResolveResultCode(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status401Unauthorized => ApiResultCodes.AccessDenied,
            StatusCodes.Status403Forbidden => ApiResultCodes.AccessDenied,
            StatusCodes.Status500InternalServerError => ApiResultCodes.ServerError,
            _ => ApiResultCodes.BusinessError
        };

    private static string ResolveFallbackLocale(string? acceptLanguage)
    => !string.IsNullOrWhiteSpace(acceptLanguage) &&
       acceptLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase)
        ? LocalizationOptions.EnglishLocale
        : LocalizationOptions.PersianLocale;

}
