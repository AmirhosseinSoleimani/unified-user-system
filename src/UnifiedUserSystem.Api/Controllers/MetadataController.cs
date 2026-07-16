using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Metadata;

namespace UnifiedUserSystem.src.Api.Controllers;

[ApiController]
[Route("api/metadata")]
public sealed class MetadataController : ControllerBase
{
    private readonly LocalizationOptions _localizationOptions;
    private readonly IRequestLocaleResolver _localeResolver;
    private readonly IGeoIpCountryResolver _geoIpCountryResolver;

    public MetadataController(
        IOptions<LocalizationOptions> localizationOptions,
        IRequestLocaleResolver localeResolver,
        IGeoIpCountryResolver geoIpCountryResolver)
    {
        _localizationOptions = localizationOptions.Value;
        _localeResolver = localeResolver;
        _geoIpCountryResolver = geoIpCountryResolver;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PlatformMetadataResponse>),StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PlatformMetadataResponse>>> Get(CancellationToken ct)
    {
        var headerLocale = _localeResolver.ResolveHeader(
            Request.Headers["Accept-Language"].ToString());

        string? countryCode = null;
        var suggestedLocale = headerLocale;

        if (suggestedLocale is null)
        {
            countryCode = await _geoIpCountryResolver.ResolveCountryCodeAsync(
                HttpContext.Connection.RemoteIpAddress,
                ct);

            suggestedLocale = ResolveLocaleFromCountry(countryCode)
                              ?? _localizationOptions.GetValidatedDefaultLocale();
        }

        var response = new PlatformMetadataResponse
        {
            SupportedLocales =
                (_localizationOptions.SupportedLocales ?? Array.Empty<string>()).ToArray(),
            DefaultLocale = _localizationOptions.GetValidatedDefaultLocale(),
            SuggestedLocale = suggestedLocale
                              ?? _localizationOptions.GetValidatedDefaultLocale(),
            CountryCode = countryCode
        };

        Response.Headers.Append("Vary", "Accept-Language");
        return Ok(ApiResponse<PlatformMetadataResponse>.Ok(response));
    }

    private string? ResolveLocaleFromCountry(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        return string.Equals(
            countryCode,
            _localizationOptions.IranCountryCode,
            StringComparison.OrdinalIgnoreCase)
            ? _localizationOptions.NormalizeSupportedLocale(
                LocalizationOptions.PersianLocale)
            : _localizationOptions.NormalizeSupportedLocale(
                LocalizationOptions.EnglishLocale);
    }
}
