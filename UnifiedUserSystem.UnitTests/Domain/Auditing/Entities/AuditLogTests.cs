using FluentAssertions;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Auditing.Entities
{
    public class AuditLogTests
    {
        [Fact]
        public void Create_Should_SetRequiredFields_When_RequestIsValid()
        {
            // Arrange
            var actorUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);

            // Act
            var result = AuditLog.Create(
                actorUserId,
                targetUserId,
                " User ",
                " 42 ",
                " Update ",
                null,
                null,
                now);

            // Assert
            result.Id.Should().NotBeEmpty();
            result.ActorUserId.Should().Be(actorUserId);
            result.TargetUserId.Should().Be(targetUserId);
            result.EntityName.Should().Be("User");
            result.EntityId.Should().Be("42");
            result.Action.Should().Be("Update");
            result.OldValues.Should().BeNull();
            result.NewValues.Should().BeNull();
            result.CreatedAt.Should().Be(now);
        }

        [Fact]
        public void Create_Should_ThrowDomainException_When_EntityNameIsEmpty()
        {
            // Act
            Action act = () => AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                " ",
                "42",
                "Update",
                null,
                null,
                DateTimeOffset.UtcNow);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("entityName is required.");
        }

        [Fact]
        public void Create_Should_ThrowDomainException_When_EntityIdIsEmpty()
        {
            // Act
            Action act = () => AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "",
                "Update",
                null,
                null,
                DateTimeOffset.UtcNow);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("entityId is required.");
        }

        [Fact]
        public void Create_Should_ThrowDomainException_When_ActionIsEmpty()
        {
            // Act
            Action act = () => AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "42",
                "   ",
                null,
                null,
                DateTimeOffset.UtcNow);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("action is required.");
        }

        [Fact]
        public void Create_Should_SupportOldAndNewSnapshots_When_BothAreProvided()
        {
            // Arrange
            var oldValues = "{\"fullname\":\"Before\"}";
            var newValues = "{\"fullname\":\"After\"}";

            // Act
            var result = AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "42",
                "Update",
                oldValues,
                newValues,
                DateTimeOffset.UtcNow);

            // Assert
            result.OldValues.Should().Be(oldValues);
            result.NewValues.Should().Be(newValues);
        }

        [Fact]
        public void Create_Should_SupportOldValuesOnly_When_NewValuesAreNotProvided()
        {
            // Arrange
            var oldValues = "{\"fullname\":\"Before\"}";

            // Act
            var result = AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "42",
                "Update",
                oldValues,
                null,
                DateTimeOffset.UtcNow);

            // Assert
            result.OldValues.Should().Be(oldValues);
            result.NewValues.Should().BeNull();
        }

        [Fact]
        public void Create_Should_SupportNewValuesOnly_When_OldValuesAreNotProvided()
        {
            // Arrange
            var newValues = "{\"fullname\":\"After\"}";

            // Act
            var result = AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "42",
                "Create",
                null,
                newValues,
                DateTimeOffset.UtcNow);

            // Assert
            result.OldValues.Should().BeNull();
            result.NewValues.Should().Be(newValues);
        }

        [Fact]
        public void Create_Should_SupportNoSnapshots_When_BothSnapshotsAreNotProvided()
        {
            // Act
            var result = AuditLog.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "User",
                "42",
                "Deactivate",
                null,
                null,
                DateTimeOffset.UtcNow);

            // Assert
            result.OldValues.Should().BeNull();
            result.NewValues.Should().BeNull();
        }
    }
}
