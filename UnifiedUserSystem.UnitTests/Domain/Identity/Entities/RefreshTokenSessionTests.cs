using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Entities
{
    public class RefreshTokenSessionTests
    {
        private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ActorUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly DateTimeOffset Now = new(2026, 06, 01, 10, 00, 00, TimeSpan.Zero);

        [Fact]
        public void RefreshTokenSession_Create_WithValidData_ShouldInitializeActiveSession()
        {
            var session = CreateSession();

            session.Id.Should().NotBeEmpty();
            session.UserId.Should().Be(UserId);
            session.RefreshTokenHash.Should().Be(TokenHash());
            session.IssuedAtUtc.Should().Be(Now);
            session.ExpiresAtUtc.Should().Be(Now.AddDays(7));
            session.RevokedAtUtc.Should().BeNull();
            session.ReplacedBySessionId.Should().BeNull();
            session.ReuseDetectedAtUtc.Should().BeNull();
            session.IsActive(Now).Should().BeTrue();
        }

        [Fact]
        public void RefreshTokenSession_Create_WithEmptyUserId_ShouldThrowDomainException()
        {
            Action act = () => RefreshTokenSession.Create(
                Guid.Empty,
                TokenHash(),
                Now,
                Now.AddDays(7),
                null,
                null,
                null,
                null,
                ActorUserId);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RefreshTokenSession_Create_WithEmptyTokenHash_ShouldThrowDomainException()
        {
            Action act = () => RefreshTokenSession.Create(
                UserId,
                "",
                Now,
                Now.AddDays(7),
                null,
                null,
                null,
                null,
                ActorUserId);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RefreshTokenSession_Create_WithInvalidExpiry_ShouldThrowDomainException()
        {
            Action act = () => RefreshTokenSession.Create(
                UserId,
                TokenHash(),
                Now,
                Now,
                null,
                null,
                null,
                null,
                ActorUserId);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RefreshTokenSession_Revoke_WhenActive_ShouldSetRevokedAtUtc()
        {
            var session = CreateSession();
            var revokedAt = Now.AddMinutes(5);

            session.Revoke(revokedAt, ActorUserId);

            session.RevokedAtUtc.Should().Be(revokedAt);
            session.IsRevoked.Should().BeTrue();
            session.IsActive(revokedAt).Should().BeFalse();
        }

        [Fact]
        public void RefreshTokenSession_Revoke_WhenAlreadyRevoked_ShouldBeIdempotent()
        {
            var session = CreateSession();
            var firstRevokedAt = Now.AddMinutes(5);
            var secondRevokedAt = Now.AddMinutes(10);

            session.Revoke(firstRevokedAt, ActorUserId);
            session.Revoke(secondRevokedAt, ActorUserId);

            session.RevokedAtUtc.Should().Be(firstRevokedAt);
        }

        [Fact]
        public void RefreshTokenSession_Rotate_WhenActive_ShouldSetReplacedBySessionIdAndRevokedAtUtc()
        {
            var session = CreateSession();
            var replacementSessionId = Guid.NewGuid();
            var rotatedAt = Now.AddMinutes(5);

            session.Rotate(replacementSessionId, rotatedAt, ActorUserId);

            session.ReplacedBySessionId.Should().Be(replacementSessionId);
            session.RevokedAtUtc.Should().Be(rotatedAt);
            session.IsActive(rotatedAt).Should().BeFalse();
        }

        [Fact]
        public void RefreshTokenSession_Rotate_WhenAlreadyRotated_ShouldThrowDomainException()
        {
            var session = CreateSession();

            session.Rotate(Guid.NewGuid(), Now.AddMinutes(5), ActorUserId);

            Action act = () => session.Rotate(Guid.NewGuid(), Now.AddMinutes(10), ActorUserId);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RefreshTokenSession_Rotate_WithEmptyReplacementSessionId_ShouldThrowDomainException()
        {
            var session = CreateSession();

            Action act = () => session.Rotate(Guid.Empty, Now.AddMinutes(5), ActorUserId);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RefreshTokenSession_MarkReuseDetected_ShouldSetReuseDetectedAtUtc()
        {
            var session = CreateSession();
            var reuseDetectedAt = Now.AddMinutes(5);

            session.MarkReuseDetected(reuseDetectedAt, ActorUserId);

            session.ReuseDetectedAtUtc.Should().Be(reuseDetectedAt);
        }

        [Fact]
        public void RefreshTokenSession_MarkReuseDetected_WhenAlreadyMarked_ShouldBeIdempotent()
        {
            var session = CreateSession();
            var firstDetectedAt = Now.AddMinutes(5);
            var secondDetectedAt = Now.AddMinutes(10);

            session.MarkReuseDetected(firstDetectedAt, ActorUserId);
            session.MarkReuseDetected(secondDetectedAt, ActorUserId);

            session.ReuseDetectedAtUtc.Should().Be(firstDetectedAt);
        }

        [Fact]
        public void RefreshTokenSession_IsActive_WhenExpired_ShouldReturnFalse()
        {
            var session = CreateSession();

            session.IsActive(Now.AddDays(7)).Should().BeFalse();
        }

        [Fact]
        public void RefreshTokenSession_IsActive_WhenRevoked_ShouldReturnFalse()
        {
            var session = CreateSession();

            session.Revoke(Now.AddMinutes(1), ActorUserId);

            session.IsActive(Now.AddMinutes(2)).Should().BeFalse();
        }

        [Fact]
        public void RefreshTokenSession_IsActive_WhenValidAndNotRevoked_ShouldReturnTrue()
        {
            var session = CreateSession();

            session.IsActive(Now.AddMinutes(1)).Should().BeTrue();
        }

        [Fact]
        public void RefreshTokenSession_Create_ShouldStoreClientMetadata_WhenProvided()
        {
            var session = RefreshTokenSession.Create(
                UserId,
                TokenHash(),
                Now,
                Now.AddDays(7),
                "Chrome on Windows",
                "Mozilla/5.0",
                "127.0.0.1",
                "web",
                ActorUserId);

            session.DeviceName.Should().Be("Chrome on Windows");
            session.UserAgent.Should().Be("Mozilla/5.0");
            session.IpAddress.Should().Be("127.0.0.1");
            session.ClientId.Should().Be("web");
        }

        [Fact]
        public void RefreshTokenSession_Create_ShouldAllowNullOptionalClientMetadata()
        {
            var session = CreateSession();

            session.DeviceName.Should().BeNull();
            session.UserAgent.Should().BeNull();
            session.IpAddress.Should().BeNull();
            session.ClientId.Should().BeNull();
        }

        private static RefreshTokenSession CreateSession()
        {
            return RefreshTokenSession.Create(
                UserId,
                TokenHash(),
                Now,
                Now.AddDays(7),
                null,
                null,
                null,
                null,
                ActorUserId);
        }

        private static string TokenHash()
        {
            return new string('a', RefreshTokenSession.RefreshTokenHashMaxLength);
        }
    }
}