using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.src.Api.Localization;

public sealed class HttpGeoIpCountryResolver : IGeoIpCountryResolver
{
    private readonly HttpClient _httpClient;
    private readonly LocalizationOptions _options;
    private readonly ILogger<HttpGeoIpCountryResolver> _logger;

    public HttpGeoIpCountryResolver(
        HttpClient httpClient,
        IOptions<LocalizationOptions> options,
        ILogger<HttpGeoIpCountryResolver> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> ResolveCountryCodeAsync(
        IPAddress? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var geoIp = _options.GeoIp;
        if (!geoIp.Enabled ||
            ipAddress is null ||
            string.IsNullOrWhiteSpace(geoIp.EndpointTemplate))
        {
            return null;
        }

        var endpoint = geoIp.EndpointTemplate.Replace(
            "{ip}",
            Uri.EscapeDataString(ipAddress.ToString()),
            StringComparison.OrdinalIgnoreCase);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(geoIp.TimeoutSeconds, 1, 10)));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrWhiteSpace(geoIp.ApiKey))
                request.Headers.TryAddWithoutValidation(geoIp.ApiKeyHeader, geoIp.ApiKey);

            using var response = await _httpClient.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);

            var root = document.RootElement;
            foreach (var propertyName in new[] { "countryCode", "country_code", "country_code2" })
            {
                if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var countryCode = value.GetString()?.Trim().ToUpperInvariant();
                    return string.IsNullOrWhiteSpace(countryCode) ? null : countryCode;
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Geo-IP lookup timed out.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Geo-IP lookup failed. The platform default locale will be used.");
        }

        return null;
    }
}

