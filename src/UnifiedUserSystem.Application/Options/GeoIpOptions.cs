
namespace UnifiedUserSystem.src.Application.Options;

public sealed class GeoIpOptions
{
    public bool Enabled { get; set; }

    public string? EndpointTemplate { get; set; }

    public string? ApiKey { get; set; }

    public string ApiKeyHeader { get; set; } = "X-Api-Key";

    public int TimeoutSeconds { get; set; } = 2;
}
