using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Auditing.Entities
{
    public class AuditLogInvariantTests
    {
        private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

        [Fact]
        public void Create_WithValidData_ShouldInitializeAuditLog_AndTrimRequiredFields()
        {
            var actor = Guid.NewGuid();
            var target = Guid.NewGuid();

            var log = AuditLog.Create(
                actor,
                target,
                " User ",
                " 123 ",
                " Updated ",
                "{\"old\":true}",
                "{\"new\":true}",
                T1);

            Assert.NotEqual(Guid.Empty, log.Id);
            Assert.Equal(actor, log.ActorUserId);
            Assert.Equal(target, log.TargetUserId);
            Assert.Equal("User", log.EntityName);
            Assert.Equal("123", log.EntityId);
            Assert.Equal("Updated", log.Action);
            Assert.Equal("{\"old\":true}", log.OldValues);
            Assert.Equal("{\"new\":true}", log.NewValues);
            Assert.Equal(T1, log.CreatedAt);
        }

        [Fact]
        public void Create_WithNullActorAndTarget_ShouldAllowSystemAuditLog()
        {
            var log = AuditLog.Create(
                null,
                null,
                "System",
                "system",
                "Started",
                null,
                null,
                T1);

            Assert.Null(log.ActorUserId);
            Assert.Null(log.TargetUserId);
            Assert.Equal("System", log.EntityName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenEntityNameIsEmpty_ShouldThrow(string entityName)
        {
            var ex = Assert.Throws<DomainException>(() =>
                AuditLog.Create(null, null, entityName!, "1", "Action", null, null, T1));

            Assert.Contains("entityName", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenEntityIdIsEmpty_ShouldThrow(string entityId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                AuditLog.Create(null, null, "Entity", entityId!, "Action", null, null, T1));

            Assert.Contains("entityId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenActionIsEmpty_ShouldThrow(string action)
        {
            var ex = Assert.Throws<DomainException>(() =>
                AuditLog.Create(null, null, "Entity", "1", action!, null, null, T1));

            Assert.Contains("action", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}