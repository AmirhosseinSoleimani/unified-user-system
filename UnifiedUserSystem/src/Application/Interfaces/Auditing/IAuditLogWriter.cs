namespace UnifiedUserSystem.src.Application.Interfaces.Auditing
{
    public interface IAuditLogWriter
    {
        Task WriteAsync(WriteAuditLogRequest request, CancellationToken ct = default);
    }
}
