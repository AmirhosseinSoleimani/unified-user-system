
namespace UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;

public sealed class AppBootstrapRequest
{
    public AppDeviceRequest? Device { get; set; }
    public string? Language { get; set; }
    public string? ErrorMessagesVersion { get; set; }
}
