namespace UnifiedUserSystem.src.Contracts.DTOs.Security;

public sealed class IpSecurityReportQuery
{
    public string? IpAddress { get; set; }
    public string? EventType { get; set; }
    public Guid? RuleId { get; set; }
    public DateTimeOffset? FromUtc { get; set; }
    public DateTimeOffset? ToUtc { get; set; }
}
