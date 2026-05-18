using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Auditing
{
    public class AuditLogRepositoryTests
    {
        [Fact]
        public async Task Add_Should_PersistAuditLog_When_SaveChangesIsCalled()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, actorUserId);
            var sut = new AuditLogRepository(dbContext);

            var auditLog = AuditLog.Create(
                actorUserId,
                targetUserId,
                "User",
                "42",
                "Update",
                "{\"fullname\":\"Before\"}",
                "{\"fullname\":\"After\"}",
                now);

            // Act
            sut.Add(auditLog);
            await dbContext.SaveChangesAsync();

            // Assert
            var persisted = await dbContext.AuditLogs.SingleAsync();
            persisted.ActorUserId.Should().Be(actorUserId);
            persisted.TargetUserId.Should().Be(targetUserId);
            persisted.EntityName.Should().Be("User");
            persisted.EntityId.Should().Be("42");
            persisted.Action.Should().Be("Update");
            persisted.OldValues.Should().Be("{\"fullname\":\"Before\"}");
            persisted.NewValues.Should().Be("{\"fullname\":\"After\"}");
            persisted.CreatedAt.Should().Be(now);
        }

        [Fact]
        public async Task WriterWithRealPersistence_Should_PersistSanitizedSnapshotsSafely()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var currentUserId = Guid.NewGuid();

            await using var dbContext = CreateDbContext(now, currentUserId);
            var auditLogRepository = new AuditLogRepository(dbContext);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepository);
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(ct => dbContext.SaveChangesAsync(ct));

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                TargetUserId = Guid.NewGuid(),
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?>
                {
                    ["Password"] = "plain-text",
                    ["fullname"] = "Before"
                },
                NewValues = new Dictionary<string, object?>
                {
                    ["PasswordHash"] = "hashed-value",
                    ["fullname"] = "After"
                }
            });

            // Assert
            var persisted = await dbContext.AuditLogs.SingleAsync();
            persisted.ActorUserId.Should().Be(currentUserId);
            persisted.EntityName.Should().Be("User");
            persisted.EntityId.Should().Be("42");
            persisted.Action.Should().Be("Update");
            persisted.CreatedAt.Should().Be(now);
            persisted.OldValues.Should().Be("{\"fullname\":\"Before\"}");
            persisted.NewValues.Should().Be("{\"fullname\":\"After\"}");
        }

        private static AppDbContext CreateDbContext(DateTimeOffset now, Guid actorUserId)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);
            currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            return new AppDbContext(options, currentUserMock.Object, clockMock.Object);
        }
    }
}
