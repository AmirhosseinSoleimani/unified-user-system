using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Domain.Security.IpRules;

internal static class IpRuleTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid ActorUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal const string ValidIpv4 = "192.168.1.10";
    internal const string ValidIpv4Cidr = "192.168.1.0/24";
    internal const string ValidIpv6 = "2001:db8::1";
    internal const string ValidReason = "Suspicious login activity";

    internal static IpRule Create(
        string ipAddressOrCidr = ValidIpv4,
        IpRuleType ruleType = IpRuleType.Block,
        string? reason = ValidReason,
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return IpRule.Create(
            ipAddressOrCidr: ipAddressOrCidr,
            ruleType: ruleType,
            reason: reason,
            nowUtc: nowUtc ?? CreatedAt,
            actorUserId: actorUserId);
    }

    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}
