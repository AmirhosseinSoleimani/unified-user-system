using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Options;

public static class ApiModelValidationOptions
{
    public static void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var httpContext = context.HttpContext;
            var resolver = httpContext.RequestServices.GetService<IRequestLocaleResolver>();
            var localizer = httpContext.RequestServices.GetService<IBusinessMessageLocalizer>()
                            ?? DictionaryBusinessMessageLocalizer.Instance;
            var locale = resolver?.Resolve(httpContext) ?? LocalizationOptions.PersianLocale;

            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => NormalizeModelStateKey(entry.Key),
                    entry => entry.Value!.Errors
                        .Select(error => LocalizeModelStateError(
                            entry.Key,
                            error.ErrorMessage,
                            localizer,
                            locale))
                        .ToArray());

            var title = localizer.Get(MessageCodes.BadRequest, locale);
            var description = errors
                .SelectMany(error => error.Value)
                .FirstOrDefault()
                ?? title;
            httpContext.Response.Headers["Content-Language"] = locale;

            return new BadRequestObjectResult(
                ApiResponse<object>.Fail(
                    title,
                    description,
                    ApiResultCodes.BusinessError,
                    httpContext.TraceIdentifier));
        };
    }

    private static string LocalizeModelStateError(
        string field,
        string errorMessage,
        IBusinessMessageLocalizer localizer,
        string locale)
    {
        var code = IsRequiredModelStateError(errorMessage)
            ? DomainErrorCodes.ValidationRequired
            : DomainErrorCodes.ValidationFailed;

        return localizer.Get(
            code,
            locale,
            new Dictionary<string, object?>
            {
                ["field"] = ResolveFieldDisplayName(field, locale)
            });
    }

    private static bool IsRequiredModelStateError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return false;

        return errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("not be null", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeModelStateKey(string key)
    {
        var fieldName = ExtractFieldName(key);

        if (string.IsNullOrWhiteSpace(fieldName))
            return "request";

        return char.ToLowerInvariant(fieldName[0]) + fieldName[1..];
    }

    private static string ResolveFieldDisplayName(string field, string locale)
    {
        var fieldName = ExtractFieldName(field);

        if (string.Equals(
                locale,
                LocalizationOptions.PersianLocale,
                StringComparison.OrdinalIgnoreCase))
        {
            return fieldName switch
            {
                "Email" => "ایمیل",
                "Username" => "نام کاربری",
                "FirstName" => "نام",
                "LastName" => "نام خانوادگی",
                "PhoneNumber" => "شماره تلفن",
                "Password" => "گذرواژه",
                _ => "درخواست"
            };
        }

        return string.IsNullOrWhiteSpace(fieldName)
            ? "request"
            : fieldName;
    }

    private static string ExtractFieldName(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return string.Empty;

        var normalized = field
            .Replace("$.", string.Empty, StringComparison.Ordinal)
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Trim('.');

        var lastSegment = normalized
            .Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .LastOrDefault();

        return lastSegment?.Trim('[', ']') ?? string.Empty;
    }
}
