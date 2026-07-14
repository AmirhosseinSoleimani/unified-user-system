using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Configuration.Entities;

public sealed class ApplicationMetadata : AuditableEntity<Guid>
{
    public static readonly Guid SingletonId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public const int VersionMaxLength = 32;
    public const int UrlMaxLength = 512;
    public const int LanguageMaxLength = 10;

    public string LatestWebVersion { get; private set; } = default!;
    public string LatestMobileVersion { get; private set; } = default!;
    public string? MinimumSupportedWebVersion { get; private set; }
    public string? MinimumSupportedMobileVersion { get; private set; }
    public string? WebUpdateUrl { get; private set; }
    public string? AndroidUpdateUrl { get; private set; }
    public string? IosUpdateUrl { get; private set; }
    public string DefaultLanguage { get; private set; } = "en";
    public bool IsMaintenanceMode { get; private set; }

    private ApplicationMetadata()
    {
    }

    public static ApplicationMetadata CreateDefault(DateTimeOffset nowUtc, Guid? actorUserId)
    {
        var metadata = new ApplicationMetadata { Id = SingletonId };

        metadata.Update(
            latestWebVersion: "1.0.0",
            latestMobileVersion: "1.0.0",
            minimumSupportedWebVersion: "1.0.0",
            minimumSupportedMobileVersion: "1.0.0",
            webUpdateUrl: null,
            androidUpdateUrl: null,
            iosUpdateUrl: null,
            defaultLanguage: "en",
            isMaintenanceMode: false,
            nowUtc: nowUtc,
            actorUserId: actorUserId);

        metadata.SetCreated(nowUtc, actorUserId);
        return metadata;
    }

    public void Update(
        string latestWebVersion,
        string latestMobileVersion,
        string? minimumSupportedWebVersion,
        string? minimumSupportedMobileVersion,
        string? webUpdateUrl,
        string? androidUpdateUrl,
        string? iosUpdateUrl,
        string defaultLanguage,
        bool isMaintenanceMode,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        latestWebVersion = NormalizeRequired(latestWebVersion, nameof(LatestWebVersion), VersionMaxLength);
        latestMobileVersion = NormalizeRequired(latestMobileVersion, nameof(LatestMobileVersion), VersionMaxLength);
        minimumSupportedWebVersion = NormalizeOptional(minimumSupportedWebVersion, nameof(MinimumSupportedWebVersion), VersionMaxLength);
        minimumSupportedMobileVersion = NormalizeOptional(minimumSupportedMobileVersion, nameof(MinimumSupportedMobileVersion), VersionMaxLength);
        webUpdateUrl = NormalizeOptional(webUpdateUrl, nameof(WebUpdateUrl), UrlMaxLength);
        androidUpdateUrl = NormalizeOptional(androidUpdateUrl, nameof(AndroidUpdateUrl), UrlMaxLength);
        iosUpdateUrl = NormalizeOptional(iosUpdateUrl, nameof(IosUpdateUrl), UrlMaxLength);
        defaultLanguage = NormalizeLanguage(defaultLanguage);

        LatestWebVersion = latestWebVersion;
        LatestMobileVersion = latestMobileVersion;
        MinimumSupportedWebVersion = minimumSupportedWebVersion;
        MinimumSupportedMobileVersion = minimumSupportedMobileVersion;
        WebUpdateUrl = webUpdateUrl;
        AndroidUpdateUrl = androidUpdateUrl;
        IosUpdateUrl = iosUpdateUrl;
        DefaultLanguage = defaultLanguage;
        IsMaintenanceMode = isMaintenanceMode;

        Touch(nowUtc, actorUserId);
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        value = (value ?? string.Empty).Trim();
        Guard.NotEmpty(value, fieldName);
        Guard.MaxLen(value, maxLength, fieldName);
        return value;
    }

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        value = value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        Guard.MaxLen(value, maxLength, fieldName);
        return value;
    }

    private static string NormalizeLanguage(string value)
    {
        value = (value ?? string.Empty).Trim();
        Guard.NotEmpty(value, nameof(DefaultLanguage));
        Guard.MaxLen(value, LanguageMaxLength, nameof(DefaultLanguage));

        return value.Equals("fa", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("fa-IR", StringComparison.OrdinalIgnoreCase)
            ? "fa"
            : "en";
    }
}
