namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IClientContext
    {
        string? DeviceName { get; }
        string? UserAgent { get; }
        string? IpAddress { get; }
        string? ClientId { get; }
    }
}
