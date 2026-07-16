
namespace UnifiedUserSystem.src.Contracts.DTOs.Metadata;

public sealed class PlatformMetadataResponse
{
    public string[] SupportedLocales { get; init; } = Array.Empty<string>();
    public string DefaultLocale { get; init; } = default!;
    public string SuggestedLocale { get; init; } = default!;
    public string? CountryCode { get; init; }
}
