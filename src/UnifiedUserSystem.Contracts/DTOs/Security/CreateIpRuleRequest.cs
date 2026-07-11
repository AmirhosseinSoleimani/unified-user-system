namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class CreateIpRuleRequest
{
    public string IpAddressOrCidr { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Block";
    public string? Reason { get; set; }
}