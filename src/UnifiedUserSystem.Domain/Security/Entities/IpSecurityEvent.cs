using System.Net;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Domain.Security.Entities;

public sealed class IpSecurityEvent : Entity<Guid>
{
    public const int IpAddressMaxLength = 64;
    public const int PathMaxLength = 512;
    public const int HttpMethodMaxLength = 16;
    public const int ReasonMaxLength = 512;

    public string IpAddress { get; private set; } = string.Empty;
    public string NormalizedIpAddress { get; private set; } = string.Empty;
    public IpSecurityEventType EventType { get; private set; }
    public string? Path { get; private set; }
    public string? HttpMethod { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? RuleId { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private IpSecurityEvent()
    {
    }

    public static IpSecurityEvent Create(
        string ipAddress,
        IpSecurityEventType eventType,
        string? path,
        string? httpMethod,
        Guid? userId,
        Guid? ruleId,
        string? reason,
        DateTimeOffset occurredAt)
    {
        var normalizedIp = NormalizeIp(ipAddress);

        Guard.True(Enum.IsDefined(typeof(IpSecurityEventType), eventType), "IP security event type is invalid.");

        if (!string.IsNullOrWhiteSpace(path))
            Guard.MaxLen(path.Trim(), PathMaxLength, nameof(Path));

        if (!string.IsNullOrWhiteSpace(httpMethod))
            Guard.MaxLen(httpMethod.Trim(), HttpMethodMaxLength, nameof(HttpMethod));

        if (!string.IsNullOrWhiteSpace(reason))
            Guard.MaxLen(reason.Trim(), ReasonMaxLength, nameof(Reason));

        return new IpSecurityEvent
        {
            Id = Guid.NewGuid(),
            IpAddress = normalizedIp,
            NormalizedIpAddress = normalizedIp,
            EventType = eventType,
            Path = string.IsNullOrWhiteSpace(path) ? null : path.Trim(),
            HttpMethod = string.IsNullOrWhiteSpace(httpMethod) ? null : httpMethod.Trim().ToUpperInvariant(),
            UserId = userId,
            RuleId = ruleId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            OccurredAt = occurredAt
        };
    }

    private static string NormalizeIp(string value)
    {
        value = Guard.NotEmpty(value, nameof(IpAddress));

        if (!IPAddress.TryParse(value, out var ip))
            throw new DomainException("IP address is invalid.");

        return ip.ToString().ToLowerInvariant();
    }
}
