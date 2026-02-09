using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Domain.Authorization.Entities
{
    public class RoleOperation : AuditableEntity<Guid>
    {
        public int RoleId { get; private set; }
        public Guid OperationId { get; private set; }
        public Role Role { get; private set; } = default!;
        public Operation Operation { get; private set; } = default!;
        private RoleOperation() { }
        public static RoleOperation Create(int roleId, Guid operationId, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            Guard.True(roleId > 0, "RoleId is invalid");
            Guard.True(operationId != Guid.Empty, "OperationId is invalid.");

            var roleOperation = new RoleOperation
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                OperationId = operationId
            };
            roleOperation.SetCreated(nowUtc, actorUserId);
            return roleOperation;
        }
    }
}
