namespace UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

public sealed record AppVersionMetadataResponse(
    string LatestWebVersion,
    string LatestMobileVersion,
    string? MinimumSupportedWebVersion,
    string? MinimumSupportedMobileVersion,
    string? WebUpdateUrl,
    string? AndroidUpdateUrl,
    string? IosUpdateUrl);
