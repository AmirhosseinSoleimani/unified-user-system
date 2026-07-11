
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Application.Models;

public sealed class IpSecurityEventSearchCriteria
{
    public string? NormalizedIpAddress { get; set; }
    public IpSecurityEventType? EventType { get; set; }
    public Guid? RuleId { get; set; }
    public DateTimeOffset? FromUtc { get; set; }
    public DateTimeOffset? ToUtc { get; set; }
}