using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Authorization.Entities
{
    public class OperationInvariantTests
    {
        private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T2 = new(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);
        private static readonly DateTimeOffset T3 = new(2026, 02, 17, 10, 20, 00, TimeSpan.Zero);

        private static string LongString(int len) => new string('a', len);

        [Fact]
        public void Create_ShouldNormalizeKey_TrimAndLower()
        {
            var op = Operation.Create(" Users.Read ", "Read Users", T1, actoractorUserId: null);

            Assert.Equal("users.read", op.Key);
        }

        [Fact]
        public void Create_ShouldNormalizeTitle_TrimOnly()
        {
            var op = Operation.Create("users.read", " Read Users ", T1, actoractorUserId: null);

            Assert.Equal("Read Users", op.Title);
        }

        [Fact]
        public void Create_ShouldSetDefaults_AndAuditFields()
        {
            var actor = Guid.NewGuid();

            var op = Operation.Create("users.read", "Read Users", T1, actor);

            Assert.NotEqual(Guid.Empty, op.Id);
            Assert.True(op.IsActive);
            Assert.False(op.IsDeleted);
            Assert.NotNull(op.RoleOperations);
            Assert.Empty(op.RoleOperations);
            Assert.Equal(T1, op.CreatedAt);
            Assert.Equal(T1, op.UpdatedAt);
            Assert.Equal(actor, op.CreatedByUserId);
            Assert.Equal(actor, op.UpdatedByUserId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenKeyIsEmpty_ShouldThrow(string key)
        {
            var ex = Assert.Throws<DomainException>(() =>
                Operation.Create(key!, "Read Users", T1, null));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("users read")]
        [InlineData("users/read")]
        [InlineData("users:read")]
        [InlineData("users@read")]
        public void Create_WhenKeyHasInvalidCharacters_ShouldThrowArgumentException(string key)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                Operation.Create(key, "Read Users", T1, null));

            Assert.Contains("invalid characters", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenKeyExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Operation.Create(LongString(Operation.KeyMaxLength + 1), "Read Users", T1, null));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenTitleIsEmpty_ShouldThrow(string title)
        {
            var ex = Assert.Throws<DomainException>(() =>
                Operation.Create("users.read", title!, T1, null));

            Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenTitleExceedsMaxLength_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Operation.Create("users.read", LongString(Operation.TitleMaxLength + 1), T1, null));

            Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeKey_ShouldNormalizeKey_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.ChangeKey(" USERS.UPDATE ", T2, actor);

            Assert.Equal("users.update", op.Key);
            Assert.Equal(T2, op.UpdatedAt);
            Assert.Equal(actor, op.UpdatedByUserId);
        }

        [Fact]
        public void ChangeKey_WhenSameAfterNormalization_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var op = Operation.Create(" Users.Read ", "Read Users", T1, actor);

            var updatedAt = op.UpdatedAt;
            var updatedBy = op.UpdatedByUserId;

            op.ChangeKey("users.read", T2, Guid.NewGuid());

            Assert.Equal("users.read", op.Key);
            Assert.Equal(updatedAt, op.UpdatedAt);
            Assert.Equal(updatedBy, op.UpdatedByUserId);
        }

        [Fact]
        public void RenameTitle_ShouldTrimTitle_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.RenameTitle(" Read Active Users ", T2, actor);

            Assert.Equal("Read Active Users", op.Title);
            Assert.Equal(T2, op.UpdatedAt);
            Assert.Equal(actor, op.UpdatedByUserId);
        }

        [Fact]
        public void RenameTitle_WhenSameAfterNormalization_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var op = Operation.Create("users.read", " Read Users ", T1, actor);

            var updatedAt = op.UpdatedAt;
            var updatedBy = op.UpdatedByUserId;

            op.RenameTitle("Read Users", T2, Guid.NewGuid());

            Assert.Equal("Read Users", op.Title);
            Assert.Equal(updatedAt, op.UpdatedAt);
            Assert.Equal(updatedBy, op.UpdatedByUserId);
        }

        [Fact]
        public void Deactive_ShouldSetInactive_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.Deactive(T2, actor);

            Assert.False(op.IsActive);
            Assert.Equal(T2, op.UpdatedAt);
            Assert.Equal(actor, op.UpdatedByUserId);
        }

        [Fact]
        public void Deactive_WhenAlreadyInactive_ShouldBeNoOp()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());

            op.Deactive(T2, Guid.NewGuid());

            var updatedAt = op.UpdatedAt;
            var updatedBy = op.UpdatedByUserId;

            op.Deactive(T3, Guid.NewGuid());

            Assert.False(op.IsActive);
            Assert.Equal(updatedAt, op.UpdatedAt);
            Assert.Equal(updatedBy, op.UpdatedByUserId);
        }

        [Fact]
        public void Active_ShouldSetActive_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.Deactive(T2, actor);
            op.Active(T3, actor);

            Assert.True(op.IsActive);
            Assert.Equal(T3, op.UpdatedAt);
            Assert.Equal(actor, op.UpdatedByUserId);
        }

        [Fact]
        public void Active_WhenAlreadyActive_ShouldBeNoOp()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());

            var updatedAt = op.UpdatedAt;
            var updatedBy = op.UpdatedByUserId;

            op.Active(T2, Guid.NewGuid());

            Assert.True(op.IsActive);
            Assert.Equal(updatedAt, op.UpdatedAt);
            Assert.Equal(updatedBy, op.UpdatedByUserId);
        }

        [Fact]
        public void Delete_ShouldSoftDelete_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.Delete(T2, actor);

            Assert.True(op.IsDeleted);
            Assert.Equal(T2, op.DeletedAt);
            Assert.Equal(actor, op.DeletedByUserId);
            Assert.Equal(T2, op.UpdatedAt);
        }

        [Fact]
        public void UnDelete_ShouldRestore_AndTouch()
        {
            var op = Operation.Create("users.read", "Read Users", T1, Guid.NewGuid());
            var actor = Guid.NewGuid();

            op.Delete(T2, actor);
            op.UnDelete(T3, actor);

            Assert.False(op.IsDeleted);
            Assert.Null(op.DeletedAt);
            Assert.Equal(T3, op.UpdatedAt);
        }

        [Fact]
        public void NormalizeKey_ShouldHandleNull_Trim_AndLower()
        {
            Assert.Equal("", Operation.NormalizeKey(null!));
            Assert.Equal("users.read", Operation.NormalizeKey(" Users.Read "));
        }

        [Fact]
        public void NormalizeTitle_ShouldHandleNull_AndTrimOnly()
        {
            Assert.Equal("", Operation.NormalizeTitle(null!));
            Assert.Equal("Read Users", Operation.NormalizeTitle(" Read Users "));
        }
    }
}