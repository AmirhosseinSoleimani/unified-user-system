using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Authorization.Entities
{
    public class RoleOperationInvariantTests
    {
        private static readonly DateTimeOffset T1 = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

        [Fact]
        public void Create_WithValidData_ShouldInitializeRoleOperation()
        {
            var operationId = Guid.NewGuid();
            var actor = Guid.NewGuid();

            var roleOperation = RoleOperation.Create(10, operationId, T1, actor);

            Assert.NotEqual(Guid.Empty, roleOperation.Id);
            Assert.Equal(10, roleOperation.RoleId);
            Assert.Equal(operationId, roleOperation.OperationId);
        }

        [Fact]
        public void Create_ShouldSetAuditFields()
        {
            var actor = Guid.NewGuid();

            var roleOperation = RoleOperation.Create(10, Guid.NewGuid(), T1, actor);

            Assert.Equal(T1, roleOperation.CreatedAt);
            Assert.Equal(T1, roleOperation.UpdatedAt);
            Assert.Equal(actor, roleOperation.CreatedByUserId);
            Assert.Equal(actor, roleOperation.UpdatedByUserId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WhenRoleIdIsInvalid_ShouldThrow(int roleId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                RoleOperation.Create(roleId, Guid.NewGuid(), T1, actorUserId: null));

            Assert.Contains("RoleId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Create_WhenOperationIdIsEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                RoleOperation.Create(1, Guid.Empty, T1, actorUserId: null));

            Assert.Contains("OperationId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}