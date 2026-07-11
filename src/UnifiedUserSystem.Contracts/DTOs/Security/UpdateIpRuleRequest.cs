
namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class UpdateIpRuleRequest
{
    public string IpAddressOrCidr { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Block";
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
}
