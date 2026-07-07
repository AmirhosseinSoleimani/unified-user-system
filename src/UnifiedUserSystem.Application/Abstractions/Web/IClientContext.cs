namespace UnifiedUserSystem.src.Application.Abstractions.Web;

public interface IClientContext
{
    string? DeviceName { get; }
    string? UserAgent { get; }
    string? IpAddress { get; }
    string? ClientId { get; }
}
