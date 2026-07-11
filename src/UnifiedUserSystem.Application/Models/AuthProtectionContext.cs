namespace UnifiedUserSystem.src.Application.Models;

public sealed class AuthProtectionContext
{
    public AuthProtectionContext(
        string loginIdentifier,
        string? clientId,
        string? ipAddress,
        string? userAgent)
    {
        LoginIdentifier = loginIdentifier;
        ClientId = clientId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public string LoginIdentifier { get; }
    public string? ClientId { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
}