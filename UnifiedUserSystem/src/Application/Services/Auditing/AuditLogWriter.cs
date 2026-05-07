using System.Text.Json;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Application.Services.Auditing
{
    public class AuditLogWriter : IAuditLogWriter
    {
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public AuditLogWriter(IUnitOfWork unitOfWork, IClock clock, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task WriteAsync(WriteAuditLogRequest request, CancellationToken ct = default)
        {
            if (request is null)
                throw new DomainException("Audit log request is null.");

            var actorUserId = request.ActorUserId ?? _currentUser.UserId;
            var oldValues = SerializeSanitized(request.OldValues);
            var newValues = SerializeSanitized(request.NewValues);

            var auditLog = AuditLog.Create(
                actorUserId,
                request.TargetUserId,
                request.EntityName,
                request.EntityId,
                request.Action,
                oldValues,
                newValues,
                _clock.Utcnow);

            _unitOfWork.AuditLogs.Add(auditLog);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static string? SerializeSanitized(IReadOnlyDictionary<string, object?>? values)
        {
            if (values is null || values.Count == 0)
                return null;

            var sanitized = values
                .Where(x => !SensitiveKeys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            if (sanitized.Count == 0)
                return null;

            return JsonSerializer.Serialize(sanitized);
        }
    }
}
