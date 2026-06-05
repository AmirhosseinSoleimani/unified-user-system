using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories;

public class RefreshTokenSessionRepositoryInfrastructureTests
{
    private static readonly DateTimeOffset BaseTime = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);

    [Fact]
    public async Task Add_Should_TrackAndPersistSession_When_SaveChangesIsCalled()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RefreshTokenSessionRepository(db);
        var session = CreateSession(userId, "hash-1");

        repository.Add(session);
        await db.SaveChangesAsync();

        var persisted = await db.RefreshTokenSessions.SingleAsync();

        persisted.Id.Should().Be(session.Id);
        persisted.UserId.Should().Be(userId);
        persisted.RefreshTokenHash.Should().Be("hash-1");
        persisted.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Add_Should_PersistOptionalClientMetadata_When_MetadataExists()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RefreshTokenSessionRepository(db);

        var session = RefreshTokenSession.Create(
            userId,
            "hash-1",
            BaseTime,
            BaseTime.AddHours(1),
            " mobile ",
            " browser ",
            " 127.0.0.1 ",
            " web ",
            userId);

        repository.Add(session);
        await db.SaveChangesAsync();

        var persisted = await db.RefreshTokenSessions.SingleAsync();

        persisted.DeviceName.Should().Be("mobile");
        persisted.UserAgent.Should().Be("browser");
        persisted.IpAddress.Should().Be("127.0.0.1");
        persisted.ClientId.Should().Be("web");
    }

    [Fact]
    public async Task FindByHashAsync_Should_ReturnSession_When_HashExists()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();
        var session = CreateSession(userId, "hash-1");

        db.RefreshTokenSessions.Add(session);
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var found = await repository.FindByHashAsync("hash-1");

        found.Should().NotBeNull();
        found!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task FindByHashAsync_Should_ReturnNull_When_HashDoesNotExist()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RefreshTokenSessionRepository(db);

        var found = await repository.FindByHashAsync("missing-hash");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByHashAsync_Should_BeExactMatch_When_HashCaseDiffers()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        db.RefreshTokenSessions.Add(CreateSession(userId, "hash-lower"));
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var found = await repository.FindByHashAsync("HASH-LOWER");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByHashAsync_Should_ReturnRevokedSession_When_HashBelongsToRevokedSession()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        var session = CreateSession(userId, "revoked-hash");
        session.Revoke(BaseTime.AddMinutes(5), userId);

        db.RefreshTokenSessions.Add(session);
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var found = await repository.FindByHashAsync("revoked-hash");

        found.Should().NotBeNull();
        found!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task ListByUserIdAsync_Should_ReturnOnlyUserSessionsOrderedByIssuedAtAscending()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        db.RefreshTokenSessions.AddRange(
            CreateSession(userId, "hash-3", issuedOffsetMinutes: 3),
            CreateSession(otherUserId, "hash-other", issuedOffsetMinutes: 1),
            CreateSession(userId, "hash-1", issuedOffsetMinutes: 1),
            CreateSession(userId, "hash-2", issuedOffsetMinutes: 2));

        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListByUserIdAsync(userId);

        sessions.Should().HaveCount(3);
        sessions.Select(x => x.RefreshTokenHash).Should().Equal("hash-1", "hash-2", "hash-3");
    }

    [Fact]
    public async Task ListByUserIdAsync_Should_IncludeRevokedAndExpiredSessions()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        var active = CreateSession(userId, "active", expiresOffsetMinutes: 60);
        var revoked = CreateSession(userId, "revoked", expiresOffsetMinutes: 60);
        revoked.Revoke(BaseTime.AddMinutes(1), userId);
        var expired = CreateSession(userId, "expired", issuedOffsetMinutes: -30, expiresOffsetMinutes: -1);

        db.RefreshTokenSessions.AddRange(active, revoked, expired);
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListByUserIdAsync(userId);

        sessions.Select(x => x.RefreshTokenHash).Should().BeEquivalentTo("active", "revoked", "expired");
    }

    [Fact]
    public async Task ListByUserIdAsync_Should_ReturnEmptyList_When_UserHasNoSessions()
    {
        await using var db = InfrastructureDbContextFactory.Create();
        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListByUserIdAsync(Guid.NewGuid());

        sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task ListActiveByUserIdAsync_Should_ReturnOnlyNotRevokedAndNotExpiredSessionsOrderedByIssuedAtDescending()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        var activeOld = CreateSession(userId, "active-old", issuedOffsetMinutes: -20, expiresOffsetMinutes: 20);
        var activeNew = CreateSession(userId, "active-new", issuedOffsetMinutes: -10, expiresOffsetMinutes: 30);
        var revoked = CreateSession(userId, "revoked", issuedOffsetMinutes: -5, expiresOffsetMinutes: 30);
        revoked.Revoke(BaseTime.AddMinutes(-1), userId);
        var expired = CreateSession(userId, "expired", issuedOffsetMinutes: -30, expiresOffsetMinutes: -1);

        db.RefreshTokenSessions.AddRange(activeOld, activeNew, revoked, expired);
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListActiveByUserIdAsync(userId, BaseTime);

        sessions.Should().HaveCount(2);
        sessions.Select(x => x.RefreshTokenHash).Should().Equal("active-new", "active-old");
    }

    [Fact]
    public async Task ListActiveByUserIdAsync_Should_ExcludeSession_When_ExpiresAtEqualsNow()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        db.RefreshTokenSessions.Add(CreateSession(userId, "expires-now", issuedOffsetMinutes: -10, expiresOffsetMinutes: 0));
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListActiveByUserIdAsync(userId, BaseTime);

        sessions.Should().BeEmpty("repository uses ExpiresAtUtc > now, so equality is expired");
    }

    [Fact]
    public async Task ListActiveByUserIdAsync_Should_ExcludeSessionsForOtherUsers()
    {
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await using var db = InfrastructureDbContextFactory.Create();

        db.RefreshTokenSessions.AddRange(
            CreateSession(targetUserId, "target-active", expiresOffsetMinutes: 60),
            CreateSession(otherUserId, "other-active", expiresOffsetMinutes: 60));

        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListActiveByUserIdAsync(targetUserId, BaseTime);

        sessions.Should().ContainSingle();
        sessions.Single().RefreshTokenHash.Should().Be("target-active");
    }

    [Fact]
    public async Task ListActiveByUserIdAsync_Should_ReturnEmptyList_When_UserHasNoActiveSessions()
    {
        var userId = Guid.NewGuid();
        await using var db = InfrastructureDbContextFactory.Create();

        var revoked = CreateSession(userId, "revoked", expiresOffsetMinutes: 60);
        revoked.Revoke(BaseTime.AddMinutes(1), userId);
        var expired = CreateSession(userId, "expired", issuedOffsetMinutes: -30, expiresOffsetMinutes: -1);

        db.RefreshTokenSessions.AddRange(revoked, expired);
        await db.SaveChangesAsync();

        var repository = new RefreshTokenSessionRepository(db);

        var sessions = await repository.ListActiveByUserIdAsync(userId, BaseTime);

        sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_Should_PersistRevocation_When_SaveChangesIsCalled()
    {
        var userId = Guid.NewGuid();
        var clock = new InfrastructureTestClock { Utcnow = BaseTime };

        await using var db = InfrastructureDbContextFactory.Create(clock, currentUserId: userId);

        var session = CreateSession(userId, "hash-1");
        db.RefreshTokenSessions.Add(session);
        await db.SaveChangesAsync();

        clock.Utcnow = BaseTime.AddMinutes(5);

        session.Revoke(clock.Utcnow, userId);
        await db.SaveChangesAsync();

        var persisted = await db.RefreshTokenSessions.SingleAsync();

        persisted.IsRevoked.Should().BeTrue();
        persisted.RevokedAtUtc.Should().Be(BaseTime.AddMinutes(5));
        persisted.UpdatedAt.Should().Be(BaseTime.AddMinutes(5));
        persisted.UpdatedByUserId.Should().Be(userId);
    }

    [Fact]
public async Task Update_Should_PersistRotation_When_SaveChangesIsCalled()
{
    var userId = Guid.NewGuid();
    var replacementId = Guid.NewGuid();
    var clock = new InfrastructureTestClock { Utcnow = BaseTime };

    await using var db = InfrastructureDbContextFactory.Create(clock);

    var session = CreateSession(userId, "hash-1");
    db.RefreshTokenSessions.Add(session);
    await db.SaveChangesAsync();

    clock.Utcnow = BaseTime.AddMinutes(5);

    session.Rotate(replacementId, clock.Utcnow, userId);
    await db.SaveChangesAsync();

    var persisted = await db.RefreshTokenSessions.SingleAsync();

    persisted.IsRevoked.Should().BeTrue();
    persisted.ReplacedBySessionId.Should().Be(replacementId);
    persisted.RevokedAtUtc.Should().Be(BaseTime.AddMinutes(5));
    persisted.UpdatedAt.Should().Be(BaseTime.AddMinutes(5));
}

    private static RefreshTokenSession CreateSession(
        Guid userId,
        string hash,
        int issuedOffsetMinutes = 0,
        int expiresOffsetMinutes = 60)
    {
        var issuedAt = BaseTime.AddMinutes(issuedOffsetMinutes);

        return RefreshTokenSession.Create(
            userId,
            hash,
            issuedAt,
            BaseTime.AddMinutes(expiresOffsetMinutes),
            null,
            null,
            null,
            null,
            userId);
    }
}