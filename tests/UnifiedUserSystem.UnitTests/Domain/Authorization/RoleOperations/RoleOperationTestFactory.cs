using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Authorization.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Authorization.RoleOperations;

internal static class RoleOperationTestFactory
{
    internal static readonly DateTimeOffset CreatedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly Guid OperationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static readonly Guid ActorUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal const int RoleId = 10;

    internal static RoleOperation Create(
        int roleId = RoleId,
        Guid? operationId = null,
        DateTimeOffset? nowUtc = null,
        Guid? actorUserId = null)
    {
        return RoleOperation.Create(
            roleId: roleId,
            operationId: operationId ?? OperationId,
            nowUtc: nowUtc ?? CreatedAt,
            actorUserId: actorUserId);
    }
}
