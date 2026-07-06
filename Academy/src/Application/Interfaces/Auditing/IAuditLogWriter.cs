namespace UnifiedUserSystem.src.Application.Interfaces.Auditing
{
    public interface IAuditLogWriter
    {
        Task AddAsync(WriteAuditLogRequest request, CancellationToken ct = default);
        Task WriteAsync(WriteAuditLogRequest request, CancellationToken ct = default);
    }
}
