using System.Globalization;
using UnifiedUserSystem.src.Api.Localization;

namespace UnifiedUserSystem.src.Api.Middlewares;

public sealed class RequestLocaleMiddleware : IMiddleware
{
    private readonly IRequestLocaleResolver _localeResolver;

    public RequestLocaleMiddleware(IRequestLocaleResolver localeResolver)
    {
        _localeResolver = localeResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var locale = _localeResolver.Resolve(context);
        var culture = CultureInfo.GetCultureInfo(locale);
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Content-Language"] = locale;
            context.Response.Headers.Append("Vary", "Accept-Language");
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
