
namespace UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

public sealed record AppDeviceResponse(
    string Platform,
    string? DeviceId,
    string? DeviceModel,
    string? OperatingSystem,
    string? AppVersion);
