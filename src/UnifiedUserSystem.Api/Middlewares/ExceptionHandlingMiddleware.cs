using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Domain.Common;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace UnifiedUserSystem.src.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly IRequestLocaleResolver _localeResolver;
    private readonly IBusinessMessageLocalizer _localizer;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware()
        : this(
            new RequestLocaleResolver(
                OptionsFactory.Create(new LocalizationOptions
            {
                DefaultLocale = LocalizationOptions.EnglishLocale,
                SupportedLocales = 
                [
                    LocalizationOptions.PersianLocale,
                    LocalizationOptions.EnglishLocale
                ]
            })),
            new DictionaryBusinessMessageLocalizer(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ExceptionHandlingMiddleware>.Instance)
    {
    }

    public ExceptionHandlingMiddleware(
        IRequestLocaleResolver localeResolver,
        IBusinessMessageLocalizer localizer,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _localeResolver = localeResolver;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (Exception exception) when (exception is ICodedBusinessException)
        {
            var businessException = (ICodedBusinessException)exception;
            var status = ResolveBusinessStatus(businessException.Code, exception);
            var resultCode = ResolveBusinessResultCode(businessException.Code, exception);

            await WriteFailureAsync(
                context,
                status,
                businessException.Code,
                businessException.Parameters,
                businessException.LegacyFallbackMessage,
                MessageCodes.DomainErrorTitle,
                resultCode);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteFailureAsync(
                context,
                StatusCodes.Status401Unauthorized,
                MessageCodes.Unauthorized,
                titleCode: MessageCodes.Unauthorized,
                resultCode: ApiResultCodes.AccessDenied);
        }
        catch (KeyNotFoundException)
        {
            await WriteFailureAsync(
                context,
                StatusCodes.Status404NotFound,
                MessageCodes.NotFound,
                titleCode: MessageCodes.NotFound,
                resultCode: ApiResultCodes.BusinessError);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "A request resulted in a conflict.");
            await WriteFailureAsync(
                context,
                StatusCodes.Status409Conflict,
                MessageCodes.Conflict,
                titleCode: MessageCodes.Conflict,
                resultCode: ApiResultCodes.BusinessError);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "A request argument was invalid.");
            await WriteFailureAsync(
                context,
                StatusCodes.Status400BadRequest,
                MessageCodes.BadRequest,
                titleCode: MessageCodes.BadRequest,
                resultCode: ApiResultCodes.BusinessError);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteFailureAsync(
                context,
                StatusCodes.Status500InternalServerError,
                MessageCodes.UnexpectedError,
                titleCode: MessageCodes.UnexpectedError,
                resultCode: ApiResultCodes.ServerError);
        }
    }

    private async Task WriteFailureAsync(
        HttpContext context,
        int status,
        string code,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string? fallbackMessage = null,
        string? titleCode = null,
        int resultCode = ApiResultCodes.BusinessError)
    {
        if (context.Response.HasStarted)
            return;

        var locale = _localeResolver.Resolve(context);
        var description = _localizer.Get(code, locale, parameters, fallbackMessage);
        var title = _localizer.Get(titleCode ?? code, locale, fallbackMessage: description);

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["Content-Language"] = locale;
        context.Response.Headers.Append("Vary", "Accept-Language");

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(
                title,
                description,
                resultCode,
                traceId: context.TraceIdentifier));
    }

    private static int ResolveBusinessStatus(string code, Exception exception)
        => code switch
        {
            DomainErrorCodes.EmailAlreadyExists => StatusCodes.Status409Conflict,
            DomainErrorCodes.UsernameAlreadyExists => StatusCodes.Status409Conflict,
            DomainErrorCodes.UserNotFound => StatusCodes.Status404NotFound,
            DomainErrorCodes.DefaultRoleNotFound => StatusCodes.Status500InternalServerError,
            _ when exception is BusinessNotFoundException => StatusCodes.Status404NotFound,
            _ when exception is BusinessConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

    private static int ResolveBusinessResultCode(string code, Exception exception)
        => code switch
        {
            DomainErrorCodes.DefaultRoleNotFound => ApiResultCodes.ServerError,
            _ => ApiResultCodes.BusinessError
        };
}