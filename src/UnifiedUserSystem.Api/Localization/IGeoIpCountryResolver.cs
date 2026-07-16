using System.Net;

namespace UnifiedUserSystem.src.Api.Localization;

public interface IGeoIpCountryResolver
{
    Task<string?> ResolveCountryCodeAsync(
        IPAddress? ipAddress,
        CancellationToken cancellationToken = default);
}
