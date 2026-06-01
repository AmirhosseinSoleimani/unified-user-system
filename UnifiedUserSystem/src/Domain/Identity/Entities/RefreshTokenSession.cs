using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Identity.Entities
{
    public class RefreshTokenSession : AuditableEntity<Guid>
    {
        public const int RefreshTokenHashMaxLength = 64;
        public const int DeviceNameMaxLength = 200;
        public const int UserAgentMaxLength = 1024;
        public const int IpAddressMaxLength = 64;
        public const int ClientIdMaxLength = 200;


        public Guid UserId { get; private set; }
        public string RefreshTokenHash { get; private set; } = default!;
        public DateTimeOffset IssuedAtUtc { get; private set; }
        public DateTimeOffset ExpiresAtUtc { get; private set; }
        public DateTimeOffset? RevokedAtUtc { get; private set; }
        public Guid? ReplacedBySessionId { get; private set; }
        public DateTimeOffset? ReuseDetectedAtUtc { get; private set; }
        public string? DeviceName { get; private set; }
        public string? UserAgent { get; private set; }
        public string? IpAddress { get; private set; }
        public string? ClientId { get; private set; }

        public User User { get; private set; } = default!;

        private RefreshTokenSession() { }

        public static RefreshTokenSession Create(
            Guid userId,
            string refreshTokenHash,
            DateTimeOffset issuedAtUtc,
            DateTimeOffset expiresAtUtc,
            string? deviceName,
            string? userAgent,
            string? ipAddress,
            string? clientId,
            Guid? actorUserId)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");
            Guard.True(expiresAtUtc > issuedAtUtc, "ExpiresAtUtc must be after IssuedAtUtc.");

            var session = new RefreshTokenSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshTokenHash = refreshTokenHash,
                IssuedAtUtc = issuedAtUtc,
                ExpiresAtUtc = expiresAtUtc,
                DeviceName = NormalizeOptional(deviceName, DeviceNameMaxLength, nameof(DeviceName)),
                UserAgent = NormalizeOptional(userAgent, UserAgentMaxLength, nameof(UserAgent)),
                IpAddress = NormalizeOptional(ipAddress, IpAddressMaxLength, nameof(IpAddress)),
                ClientId = NormalizeOptional(clientId, ClientIdMaxLength, nameof(ClientId))
            };

            session.SetCreated(issuedAtUtc, actorUserId ?? userId);
            return session;
        }

        public bool IsRevoked => RevokedAtUtc.HasValue;

        public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;

        public bool IsActive(DateTimeOffset nowUtc) => !IsExpired(nowUtc) && RevokedAtUtc is null;

        public void Revoke(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (RevokedAtUtc.HasValue) return;

            RevokedAtUtc = nowUtc;

            Touch(nowUtc, actorUserId ?? UserId);
        }

        public void Rotate(Guid replacedBySessionId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(replacedBySessionId != Guid.Empty, "ReplacedBySessionId is invalid.");

            ReplacedBySessionId = replacedBySessionId;

            if (!RevokedAtUtc.HasValue)
                RevokedAtUtc = nowUtc;

            Touch(nowUtc, actorUserId ?? UserId);
        }

        public void MarkReuseDetected(DateTimeOffset nowUtc, Guid? actorUserId)
        {
            if (!ReuseDetectedAtUtc.HasValue)
                ReuseDetectedAtUtc = nowUtc;

            Touch(nowUtc, actorUserId ?? UserId);
        }

        private static string? NormalizeOptional(string? value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized = value.Trim();
            Guard.MaxLen(normalized, maxLength, name);
            return normalized;
        }
    }
}
