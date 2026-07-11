
namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class IpRuleResponse
{
    public Guid Id { get; set; }
    public string IpAddressOrCidr { get; set; } = string.Empty;
    public string NormalizedIpAddressOrCidr { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
