
using System.Globalization;

namespace UnifiedUserSystem.src.Application.Options;

public sealed class LocalizationOptions
{
    public const string SectionName = "Localization";
    public const string PersianLocale = "fa-IR";
    public const string EnglishLocale = "en-US";

    public string DefaultLocale { get; set; } = PersianLocale;
    public string[] SupportedLocales { get; set; } =
    {
        PersianLocale,
        EnglishLocale
    };

    public string IranCountryCode { get; set; } = "IR";

    public GeoIpOptions GeoIp { get; set; } = new();

    public string? NormalizeSupportedLocale(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        var supportedLocales = SupportedLocales ?? Array.Empty<string>();
        var normalized = candidate.Trim().Replace('_', '-');

        var exact = supportedLocales.FirstOrDefault(
            locale => string.Equals(locale, normalized, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
            return exact;

        var neutralLanguage = normalized.Split('-', 2)[0];
        return supportedLocales.FirstOrDefault(locale =>
            string.Equals(locale.Split('-', 2)[0], neutralLanguage, StringComparison.OrdinalIgnoreCase));
    }

    public string GetValidatedDefaultLocale()
        => NormalizeSupportedLocale(DefaultLocale)
           ?? throw new InvalidOperationException(
               $"Localization default locale '{DefaultLocale}' is not included in SupportedLocales.");

    public bool IsValid()
    {
        var locales = SupportedLocales ?? Array.Empty<string>();

        if (locales.Length == 0 || locales.Any(string.IsNullOrWhiteSpace))
            return false;

        if (locales.Distinct(StringComparer.OrdinalIgnoreCase).Count() != locales.Length)
            return false;

        foreach (var locale in locales)
        {
            try
            {
                _ = CultureInfo.GetCultureInfo(locale.Replace('_', '-'));
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }
        if (NormalizeSupportedLocale(DefaultLocale) is null ||
            NormalizeSupportedLocale(PersianLocale) is null ||
            NormalizeSupportedLocale(EnglishLocale) is null)
        {
            return false;
        }

        if (!GeoIp.Enabled)
            return true;

        if (string.IsNullOrWhiteSpace(GeoIp.EndpointTemplate) ||
            !GeoIp.EndpointTemplate.Contains("{ip}", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var sampleEndpoint = GeoIp.EndpointTemplate.Replace(
            "{ip}",
            "127.0.0.1",
            StringComparison.OrdinalIgnoreCase);

        if (!Uri.TryCreate(sampleEndpoint, UriKind.Absolute, out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }


        return GeoIp.TimeoutSeconds is >= 1 and <= 10 &&
               (string.IsNullOrWhiteSpace(GeoIp.ApiKey) ||
                !string.IsNullOrWhiteSpace(GeoIp.ApiKeyHeader));
    }
}
