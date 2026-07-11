using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.RefreshTokens;

internal static class RefreshTokenSessionTestFactory
{
    internal static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    internal static readonly Guid ActorUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal static readonly DateTimeOffset IssuedAt =
        new(2026, 7, 11, 8, 0, 0, TimeSpan.Zero);

    internal static readonly DateTimeOffset ExpiresAt =
        IssuedAt.AddDays(30);


    internal static RefreshTokenSession Create(
        Guid? userId = null,
        string refreshTokenHash = "refresh-token-hash",
        DateTimeOffset? issuedAtUtc = null,
        DateTimeOffset? expiresAtUtc = null,
        string? deviceName = "Chrome",
        string? userAgent = "Mozilla",
        string? ipAddress = "127.0.0.1",
        string? clientId = "web-client",
        Guid? actorUserId = null)
    {
        return RefreshTokenSession.Create(
            userId: userId ?? UserId,
            refreshTokenHash: refreshTokenHash,
            issuedAtUtc: issuedAtUtc ?? IssuedAt,
            expiresAtUtc: expiresAtUtc ?? ExpiresAt,
            deviceName: deviceName,
            userAgent: userAgent,
            ipAddress: ipAddress,
            clientId: clientId,
            actorUserId: actorUserId);
    }


    internal static string CreateString(int length)
    {
        return new string('a', length);
    }
}
