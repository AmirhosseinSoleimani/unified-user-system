using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity
{
    public class RoleInvariantTests
    {
        private static DateTimeOffset T1 => new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        private static DateTimeOffset T2 => new DateTimeOffset(2026, 02, 17, 10, 10, 00, TimeSpan.Zero);
        private static DateTimeOffset T3 => new DateTimeOffset(2026, 02, 17, 10, 20, 00, TimeSpan.Zero);

        private static string LongString(int len) => new string('a', len);

        #region Create invariant
        [Fact]
        public void Create_ShouldNormalizeKey_TrimAndLower_AndSpacesToDash()
        {
            var role = Role.Create(" AdMin Sys ", "System Manager", T1, actorUserId: null);

            Assert.Equal("admin-sys", role.Key);
        }

        [Fact]
        public void Create_ShouldNormalizeName_TrimOnly()
        {
            var role = Role.Create("admin", " System Manager ", T1, actorUserId: null);

            Assert.Equal("System Manager", role.Name);
        }

        [Fact]
        public void Create_ShouldSetDefaults_IsActiveTrue_AndCollectionsEmpty()
        {
            var role = Role.Create("admin", "Admin", T1, actorUserId: null);

            Assert.True(role.IsActive);
            Assert.NotNull(role.UserRoles);
            Assert.NotNull(role.RoleOperations);
            Assert.Empty(role.UserRoles);
            Assert.Empty(role.RoleOperations);
        }

        [Fact]
        public void Create_ShouldSetAuditFields()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("admin", "Admin", T1, actor);

            Assert.Equal(T1, role.CreatedAt);
            Assert.Equal(T1, role.UpdatedAt);
            Assert.Equal(actor, role.CreatedByUserId);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenKeyIsEmpty_ShouldThrow(String key)
        {
            var ex = Assert.ThrowsAny<DomainException>(() =>
                Role.Create(key!, "Admin", T1, actorUserId: null));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WhenNameIsEmpty_ShouldThrow(string name)
        {
            var ex = Assert.ThrowsAny<DomainException>(() =>
                Role.Create("admin", name!, T1, actorUserId: null));

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenKeyExceedsMaxLength_ShouldThrow()
        {
            var key = LongString(Role.KeyMaxLength + 1);

            var ex = Assert.ThrowsAny<DomainException>(() =>
                Role.Create(key, "Admin", T1, actorUserId: null));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenNameExceedsMaxLength_ShouldThrow()
        {
            var name = LongString(Role.NameMaxLength + 1);

            var ex = Assert.ThrowsAny<DomainException>(() => 
                Role.Create("admin", name, T1, actorUserId: null));

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Rename invariants
        [Fact]
        public void Rename_ShouldNormalizeName_TrimOnly_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Rename(" Support Team ", T2, actor);

            Assert.Equal("Support Team", role.Name);
            Assert.Equal(T2, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void Rename_WhenSameNameAfterNormalization_ShouldBeNoOp_AndNotTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", " Support Team ", T1, actor);

            var updatedAtBefore = role.UpdatedAt;
            var updatedByBefore = role.UpdatedByUserId;

            role.Rename("Support Team", T2, actorUserId: Guid.NewGuid());

            Assert.Equal("Support Team", role.Name);
            Assert.Equal(updatedAtBefore, role.UpdatedAt);
            Assert.Equal(updatedByBefore, role.UpdatedByUserId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Rename_WhenNameIsEmpty_ShouldThrow(string newName)
        {
            var role = Role.Create("support", "Support", T1, actorUserId: Guid.NewGuid());

            var ex = Assert.ThrowsAny<DomainException>(() =>
                role.Rename(newName!, T2, actorUserId: Guid.NewGuid()));

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Rename_WhenNameExceedsMaxLength_ShouldThrow()
        {
            var role = Role.Create("support", "Support", T1, actorUserId: Guid.NewGuid());
            var newName = LongString(Role.NameMaxLength + 1);

            var ex = Assert.ThrowsAny<DomainException>(() =>
                role.Rename(newName, T2, actorUserId: Guid.NewGuid()));

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region ChangeKey invariant

        [Fact]
        public void ChangeKey_ShouldNormalizeKey_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.ChangeKey(" NeW Key ", T2, actor);

            Assert.Equal("new-key", role.Key);
            Assert.Equal(T2, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void ChangeKey_WhenSameKeyAfterNormalization_ShouldBeNoOp_AndNotTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create(" admin ", "Admin", T1, actor);

            var updatedAtBefore = role.UpdatedAt;
            var updatedByBefore = role.UpdatedByUserId;

            role.ChangeKey("ADMIN", T2, Guid.NewGuid());

            Assert.Equal("admin", role.Key);
            Assert.Equal(updatedAtBefore, role.UpdatedAt);
            Assert.Equal(updatedByBefore, role.UpdatedByUserId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ChangeKey_WhenKeyIsEmpty_ShouldThrow(string newKey)
        {
            var role = Role.Create("support", "Support", T1, actorUserId: Guid.NewGuid());

            var ex = Assert.ThrowsAny<DomainException>(() =>
                role.ChangeKey(newKey!, T2, actorUserId: Guid.NewGuid()));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeKey_WhenKeyExceedsMaxLength_ShouldThrow()
        {
            var role = Role.Create("support", "Support", T1, actorUserId: Guid.NewGuid());
            var newKey = LongString(Role.KeyMaxLength + 1);

            var ex = Assert.ThrowsAny<DomainException>(() =>
                role.ChangeKey(newKey, T2, actorUserId: Guid.NewGuid()));

            Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Activeate / Deactivate invariants

        [Fact]
        public void Deactivate_ShouldSetIsActiveFalse_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Deactivate(T2, actor);

            Assert.False(role.IsActive);
            Assert.Equal(T2, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Deactivate(T2, actor);
            var updatedAtAfterFirst = role.UpdatedAt;
            var updatedByAfterFirst = role.UpdatedByUserId;

            role.Deactivate(T3, Guid.NewGuid());

            Assert.False(role.IsActive);
            Assert.Equal(updatedAtAfterFirst, role.UpdatedAt);
            Assert.Equal(updatedByAfterFirst, role.UpdatedByUserId);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveTrue_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Deactivate(T2, actor);
            role.Activate(T3, actor);

            Assert.True(role.IsActive);
            Assert.Equal(T3, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            var updatedAtBefore = role.UpdatedAt;
            var updatedByBefore = role.UpdatedByUserId;

            role.Activate(T2, Guid.NewGuid());

            Assert.True(role.IsActive);
            Assert.Equal(updatedAtBefore, role.UpdatedAt);
            Assert.Equal(updatedByBefore, role.UpdatedByUserId);
        }

        #endregion

        #region SoftDelete / Restore invariants
        [Fact]
        public void Delete_shouldMarkAsDeleted_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Delete(T2, actor);

            Assert.True(role.IsDeleted);
            Assert.Equal(T2, role.DeletedAt);
            Assert.Equal(actor, role.DeletedByUserId);
            Assert.Equal(T2, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Delete(T2, actor);

            var updatedAtAfterFirst = role.UpdatedAt;
            var deletedAtAfterFirst = role.DeletedAt;
            var deletedByAfterFirst = role.DeletedByUserId;

            role.Delete(T3, Guid.NewGuid());

            Assert.True(role.IsDeleted);
            Assert.Equal(updatedAtAfterFirst, role.UpdatedAt);
            Assert.Equal(deletedAtAfterFirst, role.DeletedAt);
            Assert.Equal(deletedByAfterFirst, role.DeletedByUserId);
        }

        [Fact]
        public void UnDelete_ShoildRestore_AndTouch()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            role.Delete(T2, actor);
            role.UnDelete(T3, actor);

            Assert.False(role.IsDeleted);
            Assert.Null(role.DeletedAt);
            Assert.Equal(T3, role.UpdatedAt);
            Assert.Equal(actor, role.UpdatedByUserId);
        }

        [Fact]
        public void UnDelete_WhenNotDeleted_ShouldBeNoOp()
        {
            var actor = Guid.NewGuid();
            var role = Role.Create("support", "Support", T1, actor);

            var updatedAtBefore = role.UpdatedAt;
            var updatedByBefore = role.UpdatedByUserId;

            role.UnDelete(T2, Guid.NewGuid());

            Assert.False(role.IsDeleted);
            Assert.Equal(updatedAtBefore, role.UpdatedAt);
            Assert.Equal(updatedByBefore, role.UpdatedByUserId);
        }
        #endregion

        #region Normalize Function

        [Fact]
        public void NormalizeKey_ShouldHandleNull_TrimLower_AndSpacesToDash()
        {
            Assert.Equal("", Role.NormalizeKey(null!));
            Assert.Equal("admin", Role.NormalizeKey(" adMiN "));
            Assert.Equal("a-b-c", Role.NormalizeKey(" A B C "));
        }

        [Fact]
        public void NormalizeName_ShouldHandleNull_AndTrimOnly()
        {
            Assert.Equal("", Role.NormalizeName(null!));
            Assert.Equal("Admin", Role.NormalizeName(" Admin "));
        }
        #endregion
    }
}
