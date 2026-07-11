
using System.Net;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Domain.Security.Entities;

public sealed class IpRule : AuditableEntity<Guid>
{
    public const int IpAddressOrCidrMaxLength = 64;
    public const int ReasonMaxLength = 512;

    public string IpAddressOrCidr { get; private set; } = string.Empty;
    public string NormalizedIpAddressOrCidr { get; private set; } = string.Empty;
    public IpRuleType RuleType { get; private set; }
    public string? Reason { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }

    private IpRule()
    {
    }

    public static IpRule Create(
        string ipAddressOrCidr,
        IpRuleType ruleType,
        string? reason,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        var rule = new IpRule
        {
            Id = Guid.NewGuid(),
            IsActive = true
        };

        rule.Apply(ipAddressOrCidr, ruleType, reason);
        rule.SetCreated(nowUtc, actorUserId);

        return rule;
    }

    public void Update(
        string ipAddressOrCidr,
        IpRuleType ruleType,
        string? reason,
        bool isActive,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        Apply(ipAddressOrCidr, ruleType, reason);

        if (IsActive && !isActive)
            DisabledAt = nowUtc;
        else if (!IsActive && isActive)
            DisabledAt = null;

        IsActive = isActive;
        Touch(nowUtc, actorUserId);
    }

    public void Disable(DateTimeOffset nowUtc, Guid? actorUserId)
    {
        if (!IsActive)
            throw new DomainException("IP rule is already disabled.");

        IsActive = false;
        DisabledAt = nowUtc;
        Touch(nowUtc, actorUserId);
    }

    private void Apply(string ipAddressOrCidr, IpRuleType ruleType, string? reason)
    {
        var normalized = NormalizeIpRule(ipAddressOrCidr);

        Guard.True(Enum.IsDefined(typeof(IpRuleType), ruleType), "IP rule type is invalid.");
        Guard.MaxLen(normalized, IpAddressOrCidrMaxLength, nameof(IpAddressOrCidr));

        if (!string.IsNullOrWhiteSpace(reason))
            Guard.MaxLen(reason.Trim(), ReasonMaxLength, nameof(Reason));

        IpAddressOrCidr = normalized;
        NormalizedIpAddressOrCidr = normalized;
        RuleType = ruleType;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    private static string NormalizeIpRule(string value)
    {
        value = Guard.NotEmpty(value, nameof(IpAddressOrCidr)).ToLowerInvariant();

        var slashIndex = value.IndexOf('/');
        if (slashIndex < 0)
        {
            if (!IPAddress.TryParse(value, out var ip))
                throw new DomainException("IP address or CIDR is invalid.");

            return ip.ToString().ToLowerInvariant();
        }

        var ipPart = value[..slashIndex];
        var prefixPart = value[(slashIndex + 1)..];

        if (!IPAddress.TryParse(ipPart, out var cidrIp) || !int.TryParse(prefixPart, out var prefixLength))
            throw new DomainException("IP address or CIDR is invalid.");

        var maxPrefix = cidrIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
        if (prefixLength < 0 || prefixLength > maxPrefix)
            throw new DomainException("IP address or CIDR is invalid.");

        return $"{cidrIp}/{prefixLength}".ToLowerInvariant();
    }
}
