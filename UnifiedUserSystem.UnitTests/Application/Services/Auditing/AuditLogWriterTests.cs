using FluentAssertions;
using Moq;
using System.Text.Json;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services.Auditing
{
    public class AuditLogWriterTests
    {
        [Fact]
        public async Task AuditLogWriter_AddAsync_ShouldStageAuditLogEntry()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            await sut.AddAsync(new WriteAuditLogRequest
            {
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?> { ["Fullname"] = "Before" },
                NewValues = new Dictionary<string, object?> { ["Fullname"] = "After" }
            });

            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.ActorUserId.Should().Be(actorUserId);
            capturedAuditLog.TargetUserId.Should().Be(targetUserId);
            capturedAuditLog.EntityName.Should().Be("User");
            capturedAuditLog.EntityId.Should().Be("42");
            capturedAuditLog.Action.Should().Be("Update");
            capturedAuditLog.CreatedAt.Should().Be(now);

            var oldValues = ReadJsonObject(capturedAuditLog.OldValues!);
            oldValues.Should().ContainKey("Fullname").WhoseValue.Should().Be("Before");

            var newValues = ReadJsonObject(capturedAuditLog.NewValues!);
            newValues.Should().ContainKey("Fullname").WhoseValue.Should().Be("After");

            auditLogRepositoryMock.Verify(x => x.Add(It.IsAny<AuditLog>()), Times.Once);
        }

        [Fact]
        public async Task AuditLogWriter_AddAsync_ShouldNotCallSaveChanges()
        {
            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            await sut.AddAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update"
            });

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AuditLogWriter_WriteAsync_ShouldNotCallSaveChanges()
        {
            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update"
            });

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AuditLogWriter_AddAsync_WithInvalidInput_ShouldThrow()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var clockMock = new Mock<IClock>();
            var currentUserMock = new Mock<ICurrentUser>();

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            var act = async () => await sut.AddAsync(null!);

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Audit log request is null.");

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AuditLogWriter_AddAsync_ShouldUseCurrentUserAsActor_WhenActorIsNotProvided()
        {
            var currentUserId = Guid.NewGuid();
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            await sut.AddAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Read"
            });

            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.ActorUserId.Should().Be(currentUserId);
        }

        [Fact]
        public async Task AuditLogWriter_AddAsync_ShouldRemoveSensitiveSnapshotKeys()
        {
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object);

            await sut.AddAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?>
                {
                    ["Password"] = "plain-text",
                    ["PasswordHash"] = "hash",
                    ["Fullname"] = "Before"
                },
                NewValues = new Dictionary<string, object?>
                {
                    ["RefreshToken"] = "refresh-token",
                    ["AccessToken"] = "access-token",
                    ["Fullname"] = "After"
                }
            });

            capturedAuditLog.Should().NotBeNull();

            var oldValues = ReadJsonObject(capturedAuditLog!.OldValues!);
            oldValues.Should().ContainKey("Fullname");
            oldValues.Keys.Should().NotContain("Password");
            oldValues.Keys.Should().NotContain("PasswordHash");

            var newValues = ReadJsonObject(capturedAuditLog.NewValues!);
            newValues.Should().ContainKey("Fullname");
            newValues.Keys.Should().NotContain("RefreshToken");
            newValues.Keys.Should().NotContain("AccessToken");
        }

        private static Dictionary<string, string?> ReadJsonObject(string json)
        {
            using var document = JsonDocument.Parse(json);

            return document.RootElement
                .EnumerateObject()
                .ToDictionary(
                    x => x.Name,
                    x => x.Value.ValueKind == JsonValueKind.Null ? null : x.Value.ToString());
        }
    }
}