using UnifiedUserSystem.src.Domain.Common;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Common
{
    public class AuditableEntityInvariantTests
    {
        private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T3 = new(2026, 02, 17, 10, 20, 00, TimeSpan.Zero);

        [Fact]
        public void SetCreated_ShouldSetCreatedAndUpdatedFields()
        {
            var actor = Guid.NewGuid();
            var entity = new TestEntity();

            entity.SetCreated(T1, actor);

            Assert.Equal(T1, entity.CreatedAt);
            Assert.Equal(T1, entity.UpdatedAt);
            Assert.Equal(actor, entity.CreatedByUserId);
            Assert.Equal(actor, entity.UpdatedByUserId);
            Assert.False(entity.IsDeleted);
            Assert.Null(entity.DeletedAt);
            Assert.Null(entity.DeletedByUserId);
        }

        [Fact]
        public void Touch_ShouldUpdateUpdatedFieldsOnly()
        {
            var actor1 = Guid.NewGuid();
            var actor2 = Guid.NewGuid();
            var entity = new TestEntity();

            entity.SetCreated(T1, actor1);
            entity.Touch(T2, actor2);

            Assert.Equal(T1, entity.CreatedAt);
            Assert.Equal(actor1, entity.CreatedByUserId);
            Assert.Equal(T2, entity.UpdatedAt);
            Assert.Equal(actor2, entity.UpdatedByUserId);
        }

        [Fact]
        public void SoftDelete_ShouldMarkDeleted_AndTouch()
        {
            var actor = Guid.NewGuid();
            var entity = new TestEntity();

            entity.SetCreated(T1, actor);
            entity.SoftDelete(T2, actor);

            Assert.True(entity.IsDeleted);
            Assert.Equal(T2, entity.DeletedAt);
            Assert.Equal(actor, entity.DeletedByUserId);
            Assert.Equal(T2, entity.UpdatedAt);
            Assert.Equal(actor, entity.UpdatedByUserId);
        }

        [Fact]
        public void SoftDelete_WhenAlreadyDeleted_ShouldBeNoOp()
        {
            var entity = new TestEntity();

            entity.SetCreated(T1, Guid.NewGuid());
            entity.SoftDelete(T2, Guid.NewGuid());

            var deletedAt = entity.DeletedAt;
            var deletedBy = entity.DeletedByUserId;
            var updatedAt = entity.UpdatedAt;
            var updatedBy = entity.UpdatedByUserId;

            entity.SoftDelete(T3, Guid.NewGuid());

            Assert.Equal(deletedAt, entity.DeletedAt);
            Assert.Equal(deletedBy, entity.DeletedByUserId);
            Assert.Equal(updatedAt, entity.UpdatedAt);
            Assert.Equal(updatedBy, entity.UpdatedByUserId);
        }

        [Fact]
        public void Restore_ShouldClearDeletedState_AndTouch()
        {
            var actor = Guid.NewGuid();
            var entity = new TestEntity();

            entity.SetCreated(T1, actor);
            entity.SoftDelete(T2, actor);
            entity.Restore(T3, actor);

            Assert.False(entity.IsDeleted);
            Assert.Null(entity.DeletedAt);
            Assert.Equal(T3, entity.UpdatedAt);
            Assert.Equal(actor, entity.UpdatedByUserId);
        }

        [Fact]
        public void Restore_WhenNotDeleted_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var entity = new TestEntity();

            entity.SetCreated(T1, actor);

            var updatedAt = entity.UpdatedAt;
            var updatedBy = entity.UpdatedByUserId;

            entity.Restore(T2, Guid.NewGuid());

            Assert.False(entity.IsDeleted);
            Assert.Equal(updatedAt, entity.UpdatedAt);
            Assert.Equal(updatedBy, entity.UpdatedByUserId);
        }

        private sealed class TestEntity : AuditableEntity<Guid>
        {
        }
    }
}