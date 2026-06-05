using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Auditing;

public class AuditLogRepositoryTests
{
    private static readonly DateTimeOffset T1 = new(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);

    [Fact]
    public async Task Add_Should_PersistAuditLog_When_SaveChangesIsCalled()
    {
        var actorUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        await using var dbContext = InfrastructureDbContextFactory.Create(currentUserId: actorUserId);
        var sut = new AuditLogRepository(dbContext);

        var auditLog = AuditLog.Create(
            actorUserId,
            targetUserId,
            "User",
            "42",
            "Update",
            "{\"fullname\":\"Before\"}",
            "{\"fullname\":\"After\"}",
            T1);

        sut.Add(auditLog);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.Id.Should().NotBe(Guid.Empty);
        persisted.ActorUserId.Should().Be(actorUserId);
        persisted.TargetUserId.Should().Be(targetUserId);
        persisted.EntityName.Should().Be("User");
        persisted.EntityId.Should().Be("42");
        persisted.Action.Should().Be("Update");
        persisted.OldValues.Should().Be("{\"fullname\":\"Before\"}");
        persisted.NewValues.Should().Be("{\"fullname\":\"After\"}");
        persisted.CreatedAt.Should().Be(T1);
    }

    [Fact]
    public async Task Add_Should_TrimEntityNameEntityIdAndAction_When_AuditLogIsCreated()
    {
        await using var dbContext = InfrastructureDbContextFactory.Create();
        var sut = new AuditLogRepository(dbContext);

        var auditLog = AuditLog.Create(
            null,
            null,
            " User ",
            " 42 ",
            " Update ",
            null,
            null,
            T1);

        sut.Add(auditLog);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.EntityName.Should().Be("User");
        persisted.EntityId.Should().Be("42");
        persisted.Action.Should().Be("Update");
    }

    [Fact]
    public async Task Add_Should_PersistNullActorTargetAndSnapshots_When_ValuesAreNull()
    {
        await using var dbContext = InfrastructureDbContextFactory.Create();
        var sut = new AuditLogRepository(dbContext);

        var auditLog = AuditLog.Create(
            null,
            null,
            "System",
            "system",
            "Run",
            null,
            null,
            T1);

        sut.Add(auditLog);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.ActorUserId.Should().BeNull();
        persisted.TargetUserId.Should().BeNull();
        persisted.OldValues.Should().BeNull();
        persisted.NewValues.Should().BeNull();
    }

    [Fact]
    public async Task Add_Should_PersistMultipleAuditLogs_When_MultipleLogsAreAdded()
    {
        await using var dbContext = InfrastructureDbContextFactory.Create();
        var sut = new AuditLogRepository(dbContext);

        sut.Add(AuditLog.Create(Guid.NewGuid(), null, "User", "1", "Create", null, "{}", T1));
        sut.Add(AuditLog.Create(Guid.NewGuid(), null, "Role", "2", "Delete", "{}", null, T1.AddMinutes(1)));

        await dbContext.SaveChangesAsync();

        var logs = await dbContext.AuditLogs.OrderBy(x => x.CreatedAt).ToListAsync();

        logs.Should().HaveCount(2);
        logs.Select(x => x.EntityName).Should().Equal("User", "Role");
        logs.Select(x => x.Action).Should().Equal("Create", "Delete");
    }

    [Theory]
    [InlineData(null, "42", "Update")]
    [InlineData("", "42", "Update")]
    [InlineData(" ", "42", "Update")]
    [InlineData("User", null, "Update")]
    [InlineData("User", "", "Update")]
    [InlineData("User", " ", "Update")]
    [InlineData("User", "42", null)]
    [InlineData("User", "42", "")]
    [InlineData("User", "42", " ")]
    public void Create_Should_ThrowDomainException_When_RequiredAuditFieldsAreEmpty(
        string? entityName,
        string? entityId,
        string? action)
    {
        var act = () => AuditLog.Create(
            Guid.NewGuid(),
            null,
            entityName!,
            entityId!,
            action!,
            null,
            null,
            T1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public async Task WriterWithRealPersistence_Should_PersistSanitizedSnapshotsSafely()
    {
        var currentUserId = Guid.NewGuid();

        await using var dbContext = InfrastructureDbContextFactory.Create(currentUserId: currentUserId);
        var auditLogRepository = new AuditLogRepository(dbContext);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepository);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => dbContext.SaveChangesAsync(ct));

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(T1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

        var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

        await sut.WriteAsync(new WriteAuditLogRequest
        {
            TargetUserId = Guid.NewGuid(),
            EntityName = "User",
            EntityId = "42",
            Action = "Update",
            OldValues = new Dictionary<string, object?>
            {
                ["Password"] = "plain-text",
                ["Token"] = "token",
                ["fullname"] = "Before"
            },
            NewValues = new Dictionary<string, object?>
            {
                ["PasswordHash"] = "hashed-value",
                ["AccessToken"] = "access-token",
                ["fullname"] = "After"
            }
        });

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.ActorUserId.Should().Be(currentUserId);
        persisted.EntityName.Should().Be("User");
        persisted.EntityId.Should().Be("42");
        persisted.Action.Should().Be("Update");
        persisted.CreatedAt.Should().Be(T1);
        persisted.OldValues.Should().Be("{\"fullname\":\"Before\"}");
        persisted.NewValues.Should().Be("{\"fullname\":\"After\"}");
    }

    [Fact]
    public async Task WriterWithRealPersistence_Should_UseExplicitActorUserId_When_RequestProvidesActor()
    {
        var currentUserId = Guid.NewGuid();
        var explicitActorId = Guid.NewGuid();

        await using var dbContext = InfrastructureDbContextFactory.Create(currentUserId: currentUserId);
        var auditLogRepository = new AuditLogRepository(dbContext);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepository);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => dbContext.SaveChangesAsync(ct));

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(T1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);

        var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

        await sut.WriteAsync(new WriteAuditLogRequest
        {
            ActorUserId = explicitActorId,
            EntityName = "User",
            EntityId = "42",
            Action = "Update"
        });

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.ActorUserId.Should().Be(explicitActorId);
    }

    [Fact]
    public async Task WriterWithRealPersistence_Should_PersistNullSnapshots_When_AllValuesAreSensitive()
    {
        var currentUserId = Guid.NewGuid();

        await using var dbContext = InfrastructureDbContextFactory.Create(currentUserId: currentUserId);
        var auditLogRepository = new AuditLogRepository(dbContext);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepository);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => dbContext.SaveChangesAsync(ct));

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Utcnow).Returns(T1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);

        var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

        await sut.WriteAsync(new WriteAuditLogRequest
        {
            EntityName = "User",
            EntityId = "42",
            Action = "Update",
            OldValues = new Dictionary<string, object?> { ["Password"] = "plain" },
            NewValues = new Dictionary<string, object?> { ["RefreshToken"] = "refresh" }
        });

        var persisted = await dbContext.AuditLogs.SingleAsync();

        persisted.OldValues.Should().BeNull();
        persisted.NewValues.Should().BeNull();
    }

    [Fact]
    public async Task WriterWithRealPersistence_Should_ThrowDomainException_When_RequestIsNull()
    {
        await using var dbContext = InfrastructureDbContextFactory.Create();
        var auditLogRepository = new AuditLogRepository(dbContext);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepository);

        var clockMock = new Mock<IClock>();
        var currentUserMock = new Mock<ICurrentUser>();

        var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

        var act = () => sut.WriteAsync(null!);

        await act.Should().ThrowAsync<DomainException>();
    }
}