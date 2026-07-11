
namespace UnifiedUserSystem.src.Application.Models;

public sealed record IpAccessDecision(
    bool IsAllowed,
    Guid? RuleId,
    string? Reason)
{
    public static IpAccessDecision Allowed()
        => new(true, null, null);

    public static IpAccessDecision Blocked(Guid ruleId, string? reason)
        => new(false, ruleId, reason ?? "IP address is blocked.");
}
