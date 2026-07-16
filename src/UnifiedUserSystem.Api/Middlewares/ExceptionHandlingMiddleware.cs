using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Application.Options;
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
                SupportedLocales = new[]
                {
                    LocalizationOptions.PersianLocale,
                    LocalizationOptions.EnglishLocale
                }
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
            await WriteProblemAsync(
                context,
                status,
                businessException.Code,
                businessException.Parameters,
                businessException.LegacyFallbackMessage,
                MessageCodes.DomainErrorTitle);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                MessageCodes.Unauthorized,
                titleCode: MessageCodes.Unauthorized);
        }
        catch (KeyNotFoundException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                MessageCodes.NotFound,
                titleCode: MessageCodes.NotFound);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "A request resulted in a conflict.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                MessageCodes.Conflict,
                titleCode: MessageCodes.Conflict);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "A request argument was invalid.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                MessageCodes.BadRequest,
                titleCode: MessageCodes.BadRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                MessageCodes.UnexpectedError,
                titleCode: MessageCodes.UnexpectedError);
        }
    }

    private async Task WriteProblemAsync(
        HttpContext context,
        int status,
        string code,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string? fallbackMessage = null,
        string? titleCode = null)
    {
        if (context.Response.HasStarted)
            return;

        var locale = _localeResolver.Resolve(context);
        var detail = _localizer.Get(code, locale, parameters, fallbackMessage);
        var title = _localizer.Get(titleCode ?? code, locale, fallbackMessage: detail);

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["Content-Language"] = locale;
        context.Response.Headers.Append("Vary", "Accept-Language");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
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
}