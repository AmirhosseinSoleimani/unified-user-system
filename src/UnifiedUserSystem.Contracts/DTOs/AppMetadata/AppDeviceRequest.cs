
namespace UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

public sealed class AppDeviceRequest
{
    public string? Platform { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceModel { get; set; }
    public string? OperatingSystem { get; set; }
    public string? AppVersion { get; set; }
}
