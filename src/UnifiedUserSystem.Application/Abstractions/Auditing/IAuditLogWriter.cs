namespace UnifiedUserSystem.src.Application.Abstractions.Auditing;

public interface IAuditLogWriter
{
    Task AddAsync(WriteAuditLogRequest request, CancellationToken ct = default);
    Task WriteAsync(WriteAuditLogRequest request, CancellationToken ct = default);
}
