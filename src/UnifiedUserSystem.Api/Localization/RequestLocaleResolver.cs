using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.src.Api.Localization;

public sealed class RequestLocaleResolver
    : IRequestLocaleResolver
{
    private const string LocaleItemKey =
        "__UnifiedUserSystem.RequestLocale";

    private readonly LocalizationOptions _options;

    public RequestLocaleResolver(
        IOptions<LocalizationOptions> options)
    {
        _options = options.Value;
    }

    public string Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue(
                LocaleItemKey,
                out var cachedLocale) &&
            cachedLocale is string locale)
        {
            return locale;
        }

        var resolvedLocale =
            ResolveHeader(
                context.Request.Headers.AcceptLanguage.ToString())
            ?? _options.GetValidatedDefaultLocale();

        context.Items[LocaleItemKey] = resolvedLocale;

        return resolvedLocale;
    }

    public string? ResolveHeader(string? acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return null;

        var candidates = ParseAcceptLanguage(acceptLanguage);

        foreach (var candidate in candidates)
        {
            var supportedLocale =
                _options.NormalizeSupportedLocale(
                    candidate.Locale);

            if (supportedLocale is not null)
                return supportedLocale;
        }

        return null;
    }

    private static IEnumerable<LocaleCandidate>
        ParseAcceptLanguage(string header)
    {
        return header
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(ParseCandidate)
            .Where(candidate =>
                candidate is not null &&
                candidate.Locale != "*")
            .Select(candidate => candidate!)
            .OrderByDescending(candidate => candidate.Quality);
    }

    private static LocaleCandidate? ParseCandidate(
        string value)
    {
        var parts = value.Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (parts.Length == 0 ||
            string.IsNullOrWhiteSpace(parts[0]))
        {
            return null;
        }

        var quality = 1D;

        foreach (var parameter in parts.Skip(1))
        {
            if (!parameter.StartsWith(
                    "q=",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var qualityText = parameter[2..];

            if (double.TryParse(
                    qualityText,
                    System.Globalization.NumberStyles
                        .AllowDecimalPoint,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var parsedQuality))
            {
                quality = parsedQuality;
            }
        }

        return new LocaleCandidate(
            parts[0],
            quality);
    }

    private sealed record LocaleCandidate(
        string Locale,
        double Quality);
}
