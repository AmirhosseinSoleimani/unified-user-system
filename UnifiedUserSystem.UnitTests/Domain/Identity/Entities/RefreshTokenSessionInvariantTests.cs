using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Entities
{
    public class RefreshTokenSessionInvariantTests
    {
        private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T3 = new(2026, 02, 17, 10, 20, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T4 = new(2026, 02, 17, 10, 30, 00, TimeSpan.Zero);

        private static string LongString(int len) => new string('a', len);

        [Fact]
        public void Create_WithValidData_ShouldInitializeSession()
        {
            var userId = Guid.NewGuid();

            var session = RefreshTokenSession.Create(
                userId,
                "HASH",
                T1,
                T3,
                " device ",
                " agent ",
                " 127.0.0.1 ",
                " client ",
                actorUserId: userId);

            Assert.NotEqual(Guid.Empty, session.Id);
            Assert.Equal(userId, session.UserId);
            Assert.Equal("HASH", session.RefreshTokenHash);
            Assert.Equal(T1, session.IssuedAtUtc);
            Assert.Equal(T3, session.ExpiresAtUtc);
            Assert.Null(session.RevokedAtUtc);
            Assert.Null(session.ReplacedBySessionId);
            Assert.Null(session.ReuseDetectedAtUtc);
            Assert.False(session.IsRevoked);
            Assert.False(session.IsDeleted);
        }

        [Fact]
        public void Create_ShouldNormalizeOptionalFields_Trim_AndConvertWhitespaceToNull()
        {
            var session = RefreshTokenSession.Create(
                Guid.NewGuid(),
                "HASH",
                T1,
                T3,
                " device ",
                " agent ",
                " 127.0.0.1 ",
                " ",
                actorUserId: null);

            Assert.Equal("device", session.DeviceName);
            Assert.Equal("agent", session.UserAgent);
            Assert.Equal("127.0.0.1", session.IpAddress);
            Assert.Null(session.ClientId);
        }

        [Fact]
        public void Create_ShouldSetAuditFields_UsingActorWhenProvided()
        {
            var userId = Guid.NewGuid();
            var actor = Guid.NewGuid();

            var session = RefreshTokenSession.Create(userId, "HASH", T1, T3, null, null, null, null, actor);

            Assert.Equal(T1, session.CreatedAt);
            Assert.Equal(T1, session.UpdatedAt);
            Assert.Equal(actor, session.CreatedByUserId);
            Assert.Equal(actor, session.UpdatedByUserId);
        }

        [Fact]
        public void Create_ShouldSetAuditFields_UsingUserIdWhenActorIsNull()
        {
            var userId = Guid.NewGuid();

            var session = RefreshTokenSession.Create(userId, "HASH", T1, T3, null, null, null, null, actorUserId: null);

            Assert.Equal(userId, session.CreatedByUserId);
            Assert.Equal(userId, session.UpdatedByUserId);
        }

        [Fact]
        public void Create_WhenUserIdIsEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(Guid.Empty, "HASH", T1, T3, null, null, null, null, null));

            Assert.Contains("UserId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenRefreshTokenHashIsEmpty_ShouldThrow(string hash)
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(Guid.NewGuid(), hash!, T1, T3, null, null, null, null, null));

            Assert.Contains("refreshTokenHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenRefreshTokenHashExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(
                    Guid.NewGuid(),
                    LongString(RefreshTokenSession.RefreshTokenHashMaxLength + 1),
                    T1,
                    T3,
                    null,
                    null,
                    null,
                    null,
                    null));

            Assert.Contains("refreshTokenHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenExpiresAtEqualsIssuedAt_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(Guid.NewGuid(), "HASH", T1, T1, null, null, null, null, null));

            Assert.Contains("ExpiresAtUtc", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenExpiresAtBeforeIssuedAt_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(Guid.NewGuid(), "HASH", T3, T1, null, null, null, null, null));

            Assert.Contains("ExpiresAtUtc", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenDeviceNameExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(
                    Guid.NewGuid(),
                    "HASH",
                    T1,
                    T3,
                    LongString(RefreshTokenSession.DeviceNameMaxLength + 1),
                    null,
                    null,
                    null,
                    null));

            Assert.Contains("DeviceName", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenUserAgentExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(
                    Guid.NewGuid(),
                    "HASH",
                    T1,
                    T3,
                    null,
                    LongString(RefreshTokenSession.UserAgentMaxLength + 1),
                    null,
                    null,
                    null));

            Assert.Contains("UserAgent", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenIpAddressExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(
                    Guid.NewGuid(),
                    "HASH",
                    T1,
                    T3,
                    null,
                    null,
                    LongString(RefreshTokenSession.IpAddressMaxLength + 1),
                    null,
                    null));

            Assert.Contains("IpAddress", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenClientIdExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RefreshTokenSession.Create(
                    Guid.NewGuid(),
                    "HASH",
                    T1,
                    T3,
                    null,
                    null,
                    null,
                    LongString(RefreshTokenSession.ClientIdMaxLength + 1),
                    null));

            Assert.Contains("ClientId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IsExpired_WhenNowIsBeforeExpiresAt_ShouldReturnFalse()
        {
            var session = Create();

            Assert.False(session.IsExpired(T2));
        }

        [Fact]
        public void IsExpired_WhenNowEqualsExpiresAt_ShouldReturnTrue()
        {
            var session = Create();

            Assert.True(session.IsExpired(T3));
        }

        [Fact]
        public void IsActive_WhenNotExpiredAndNotRevoked_ShouldReturnTrue()
        {
            var session = Create();

            Assert.True(session.IsActive(T2));
        }

        [Fact]
        public void IsActive_WhenExpired_ShouldReturnFalse()
        {
            var session = Create();

            Assert.False(session.IsActive(T3));
        }

        [Fact]
        public void IsActive_WhenRevoked_ShouldReturnFalse()
        {
            var session = Create();

            session.Revoke(T2, session.UserId);

            Assert.False(session.IsActive(T2.AddSeconds(1)));
        }

        [Fact]
        public void Revoke_ShouldSetRevokedAt_AndTouch()
        {
            var session = Create();

            session.Revoke(T2, session.UserId);

            Assert.True(session.IsRevoked);
            Assert.Equal(T2, session.RevokedAtUtc);
            Assert.Equal(T2, session.UpdatedAt);
            Assert.Equal(session.UserId, session.UpdatedByUserId);
        }

        [Fact]
        public void Revoke_WhenActorIsNull_ShouldTouchWithUserId()
        {
            var session = Create();

            session.Revoke(T2, actorUserId: null);

            Assert.Equal(session.UserId, session.UpdatedByUserId);
        }

        [Fact]
        public void Revoke_WhenAlreadyRevoked_ShouldBeNoOp()
        {
            var session = Create();

            session.Revoke(T2, session.UserId);

            var revokedAt = session.RevokedAtUtc;
            var updatedAt = session.UpdatedAt;
            var updatedBy = session.UpdatedByUserId;

            session.Revoke(T4, Guid.NewGuid());

            Assert.Equal(revokedAt, session.RevokedAtUtc);
            Assert.Equal(updatedAt, session.UpdatedAt);
            Assert.Equal(updatedBy, session.UpdatedByUserId);
        }

        [Fact]
        public void Rotate_ShouldSetReplacement_RevokeCurrentSession_AndTouch()
        {
            var session = Create();
            var replacementId = Guid.NewGuid();

            session.Rotate(replacementId, T2, session.UserId);

            Assert.Equal(replacementId, session.ReplacedBySessionId);
            Assert.Equal(T2, session.RevokedAtUtc);
            Assert.True(session.IsRevoked);
            Assert.Equal(T2, session.UpdatedAt);
            Assert.Equal(session.UserId, session.UpdatedByUserId);
        }

        [Fact]
        public void Rotate_WhenSessionAlreadyRevoked_ShouldKeepOriginalRevokedAt_ButSetReplacementAndTouch()
        {
            var session = Create();
            var replacementId = Guid.NewGuid();

            session.Revoke(T2, session.UserId);
            session.Rotate(replacementId, T3, session.UserId);

            Assert.Equal(replacementId, session.ReplacedBySessionId);
            Assert.Equal(T2, session.RevokedAtUtc);
            Assert.Equal(T3, session.UpdatedAt);
        }

        [Fact]
        public void Rotate_WhenReplacementIdIsEmpty_ShouldThrow()
        {
            var session = Create();

            var ex = Assert.Throws<DomainException>(() =>
                session.Rotate(Guid.Empty, T2, session.UserId));

            Assert.Contains("ReplacedBySessionId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Rotate_WhenAlreadyRotated_ShouldThrow()
        {
            var session = Create();

            session.Rotate(Guid.NewGuid(), T2, session.UserId);

            var ex = Assert.Throws<DomainException>(() =>
                session.Rotate(Guid.NewGuid(), T3, session.UserId));

            Assert.Contains("already been rotated", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void MarkReuseDetected_ShouldSetReuseDetectedAt_AndTouch()
        {
            var session = Create();

            session.MarkReuseDetected(T2, session.UserId);

            Assert.Equal(T2, session.ReuseDetectedAtUtc);
            Assert.Equal(T2, session.UpdatedAt);
            Assert.Equal(session.UserId, session.UpdatedByUserId);
        }

        [Fact]
        public void MarkReuseDetected_WhenAlreadyMarked_ShouldKeepOriginalReuseTime_ButTouchAgain()
        {
            var session = Create();

            session.MarkReuseDetected(T2, session.UserId);
            session.MarkReuseDetected(T4, session.UserId);

            Assert.Equal(T2, session.ReuseDetectedAtUtc);
            Assert.Equal(T4, session.UpdatedAt);
        }

        [Fact]
        public void SoftDelete_ShouldMarkDeleted_AndTouch()
        {
            var session = Create();

            session.SoftDelete(T2, session.UserId);

            Assert.True(session.IsDeleted);
            Assert.Equal(T2, session.DeletedAt);
            Assert.Equal(session.UserId, session.DeletedByUserId);
            Assert.Equal(T2, session.UpdatedAt);
        }

        [Fact]
        public void Restore_ShouldRestoreDeletedSession_AndTouch()
        {
            var session = Create();

            session.SoftDelete(T2, session.UserId);
            session.Restore(T3, session.UserId);

            Assert.False(session.IsDeleted);
            Assert.Null(session.DeletedAt);
            Assert.Equal(T3, session.UpdatedAt);
        }

        private static RefreshTokenSession Create()
        {
            return RefreshTokenSession.Create(
                Guid.NewGuid(),
                "HASH",
                T1,
                T3,
                null,
                null,
                null,
                null,
                actorUserId: null);
        }
    }
}