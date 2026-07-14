using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Configuration.Entities;
using UnifiedUserSystemsrc.src.Application.Options;

namespace UnifiedUserSystem.src.Application.Services.Configuration;

public sealed class ApplicationMetadataService : IApplicationMetadataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILocalizedMessageCache _localizedMessageCache;
    private readonly ApplicationMetadataDefaultsOptions _defaults;

    public ApplicationMetadataService(
        IUnitOfWork unitOfWork,
        IClock clock,
        ILocalizedMessageCache localizedMessageCache,
        IOptions<ApplicationMetadataDefaultsOptions> defaults)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _localizedMessageCache = localizedMessageCache;
        _defaults = defaults.Value;
    }

    public async Task<AppBootstrapResponse> GetBootstrapAsync(AppBootstrapRequest request, CancellationToken ct = default)
    {
        request ??= new AppBootstrapRequest();

        var metadata = await _unitOfWork.ApplicationMetadata.GetAsync(ct)
            ?? BuildDefaultMetadata();

        var language = NormalizeLanguage(request.Language, metadata.DefaultLanguage);
        var snapshot = await _localizedMessageCache.GetSnapshotAsync(ct);
        var errorMessagesChanged = !string.Equals(
            request.ErrorMessagesVersion,
            snapshot.Version,
            StringComparison.Ordinal);

        return new AppBootstrapResponse(
            Versions: new AppVersionMetadataResponse(
                LatestWebVersion: metadata.LatestWebVersion,
                LatestMobileVersion: metadata.LatestMobileVersion,
                MinimumSupportedWebVersion: metadata.MinimumSupportedWebVersion,
                MinimumSupportedMobileVersion: metadata.MinimumSupportedMobileVersion,
                WebUpdateUrl: metadata.WebUpdateUrl,
                AndroidUpdateUrl: metadata.AndroidUpdateUrl,
                IosUpdateUrl: metadata.IosUpdateUrl),
            Device: NormalizeDevice(request.Device),
            Language: language,
            IsMaintenanceMode: metadata.IsMaintenanceMode,
            ErrorMessagesVersion: snapshot.Version,
            ErrorMessagesChanged: errorMessagesChanged,
            ErrorMessages: errorMessagesChanged
                ? BuildErrorMessages(snapshot.Messages, language)
                : new Dictionary<string, LocalizedMessageResponse>());
    }

    private static IReadOnlyDictionary<string, LocalizedMessageResponse> BuildErrorMessages(
        IReadOnlyDictionary<string, LocalizedMessage> messages,
        string language
        )
    {
        return messages.ToDictionary(
            x => x.Key,
            x => new LocalizedMessageResponse(
                English: x.Value.English,
                Persian: x.Value.Persian,
                Selected: x.Value.Get(language)),
            StringComparer.OrdinalIgnoreCase);
    }

    private ApplicationMetadata BuildDefaultMetadata()
    {
        var metadata = ApplicationMetadata.CreateDefault(_clock.Utcnow, actorUserId: null);

        metadata.Update(
            latestWebVersion: _defaults.LatestWebVersion,
            latestMobileVersion: _defaults.LatestMobileVersion,
            minimumSupportedWebVersion: _defaults.MinimumSupportedWebVersion,
            minimumSupportedMobileVersion: _defaults.MinimumSupportedMobileVersion,
            webUpdateUrl: _defaults.WebUpdateUrl,
            androidUpdateUrl: _defaults.AndroidUpdateUrl,
            iosUpdateUrl: _defaults.IosUpdateUrl,
            defaultLanguage: _defaults.DefaultLanguage,
            isMaintenanceMode: _defaults.IsMaintenanceMode,
            nowUtc: _clock.Utcnow,
            actorUserId: null);

        return metadata;
    }

    private static AppDeviceResponse NormalizeDevice(AppDeviceRequest? device)
    {
        var platform = NormalizePlatform(device?.Platform);

        return new AppDeviceResponse(
            Platform: NormalizePlatform(device?.Platform),
            DeviceId: TrimToNull(device?.DeviceId),
            DeviceModel: TrimToNull(device?.DeviceModel),
            OperatingSystem: TrimToNull(device?.OperatingSystem),
            AppVersion: TrimToNull(device?.AppVersion));
    }

    private static string NormalizePlatform(string? platform)
    {
        platform = TrimToNull(platform)?.ToLowerInvariant();

        return platform switch
        {
            "web" => "web",
            "android" => "android",
            "ios" => "ios",
            _ => "unknown"
        };
    }

    private static string NormalizeLanguage(string? requestedLanguage, string defaultLanguage)
    {
        var language = TrimToNull(requestedLanguage) ?? TrimToNull(defaultLanguage) ?? "en";
        return language.StartsWith("fa", StringComparison.OrdinalIgnoreCase) ? "fa" : "en";
    }

    private static string? TrimToNull(string? value)
    {
        value = value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
