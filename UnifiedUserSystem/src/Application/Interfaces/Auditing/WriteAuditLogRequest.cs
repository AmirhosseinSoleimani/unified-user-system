namespace UnifiedUserSystem.src.Application.Interfaces.Auditing
{
    public class WriteAuditLogRequest
    {
        public Guid? ActorUserId { get; set; }
        public Guid? TargetUserId { get; set; }
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Action { get; set; } = default!;
        public IReadOnlyDictionary<string, object?>? OldValues { get; set; }
        public IReadOnlyDictionary<string, object?>? NewValues { get; set; }
    }
}
