
namespace UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

public sealed record AppBootstrapResponse(
    AppVersionMetadataResponse Versions,
    AppDeviceResponse Device,
    string Language,
    bool IsMaintenanceMode,
    string ErrorMessagesVersion,
    bool ErrorMessagesChanged,
    IReadOnlyDictionary<string, LocalizedMessageResponse> ErrorMessages);
