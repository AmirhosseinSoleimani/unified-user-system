
namespace UnifiedUserSystemsrc.src.Application.Options;

public sealed class ApplicationMetadataDefaultsOptions
{
    public string LatestWebVersion { get; set; } = "1.0.0";
    public string LatestMobileVersion { get; set; } = "1.0.0";
    public string? MinimumSupportedWebVersion { get; set; } = "1.0.0";
    public string? MinimumSupportedMobileVersion { get; set; } = "1.0.0";
    public string? WebUpdateUrl { get; set; }
    public string? AndroidUpdateUrl { get; set; }
    public string? IosUpdateUrl { get; set; }
    public string DefaultLanguage { get; set; } = "en";
    public bool IsMaintenanceMode { get; set; }
}
