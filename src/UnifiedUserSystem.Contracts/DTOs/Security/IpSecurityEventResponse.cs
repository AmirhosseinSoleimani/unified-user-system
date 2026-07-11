
namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class IpSecurityEventResponse
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string NormalizedIpAddress { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? HttpMethod { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RuleId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
