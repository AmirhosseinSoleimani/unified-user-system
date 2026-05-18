using FluentAssertions;
using Moq;
using System.Text.Json;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.Application.Services.Auditing
{
    public class AuditLogWriterTests
    {
        [Fact]
        public async Task WriteAsync_Should_ThrowDomainException_When_RequestIsNull()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var clockMock = new Mock<IClock>();
            var currentUserMock = new Mock<ICurrentUser>();

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            Func<Task> act = async () => await sut.WriteAsync(null!, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Audit log request is null.");
        }

        [Fact]
        public async Task WriteAsync_Should_CreateAuditLogAndPersistIt_When_RequestIsValid()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var ct = new CancellationTokenSource().Token;
            AuditLog? capturedAuditLog = null;

            var request = new WriteAuditLogRequest
            {
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?> { ["fullname"] = "Before" },
                NewValues = new Dictionary<string, object?> { ["fullname"] = "After" }
            };

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(request, ct);

            // Assert
            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.ActorUserId.Should().Be(actorUserId);
            capturedAuditLog.TargetUserId.Should().Be(targetUserId);
            capturedAuditLog.EntityName.Should().Be("User");
            capturedAuditLog.EntityId.Should().Be("42");
            capturedAuditLog.Action.Should().Be("Update");
            capturedAuditLog.CreatedAt.Should().Be(now);

            var oldValues = ReadJsonObject(capturedAuditLog.OldValues!);
            oldValues.Should().ContainKey("fullname").WhoseValue.Should().Be("Before");

            var newValues = ReadJsonObject(capturedAuditLog.NewValues!);
            newValues.Should().ContainKey("fullname").WhoseValue.Should().Be("After");

            auditLogRepositoryMock.Verify(x => x.Add(It.IsAny<AuditLog>()), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task WriteAsync_Should_UseCurrentUserAsActor_When_RequestActorIsNotProvided()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero));

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(currentUserId);

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Read"
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.ActorUserId.Should().Be(currentUserId);
        }

        [Fact]
        public async Task WriteAsync_Should_PersistNullSnapshots_When_RequestHasNoSnapshots()
        {
            // Arrange
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Read"
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.OldValues.Should().BeNull();
            capturedAuditLog.NewValues.Should().BeNull();
        }

        [Fact]
        public async Task WriteAsync_Should_PersistOnlyOldValues_When_NewValuesAreNotProvided()
        {
            // Arrange
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?> { ["fullname"] = "Before" }
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();
            var oldValues = ReadJsonObject(capturedAuditLog!.OldValues!);
            oldValues.Should().ContainKey("fullname").WhoseValue.Should().Be("Before");
            capturedAuditLog.NewValues.Should().BeNull();
        }

        [Fact]
        public async Task WriteAsync_Should_PersistOnlyNewValues_When_OldValuesAreNotProvided()
        {
            // Arrange
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Create",
                NewValues = new Dictionary<string, object?> { ["fullname"] = "After" }
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.OldValues.Should().BeNull();
            var newValues = ReadJsonObject(capturedAuditLog.NewValues!);
            newValues.Should().ContainKey("fullname").WhoseValue.Should().Be("After");
        }

        [Fact]
        public async Task WriteAsync_Should_RemovePasswordFields_When_SerializingSnapshots()
        {
            // Arrange
            AuditLog? capturedAuditLog = null;

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = new Dictionary<string, object?>
                {
                    ["Password"] = "plain-text",
                    ["PasswordHash"] = "hashed-value",
                    ["fullname"] = "Before"
                },
                NewValues = new Dictionary<string, object?>
                {
                    ["password"] = "new-plain-text",
                    ["passwordhash"] = "new-hashed-value",
                    ["fullname"] = "After"
                }
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();

            var oldValues = ReadJsonObject(capturedAuditLog!.OldValues!);
            oldValues.Should().ContainKey("fullname").WhoseValue.Should().Be("Before");
            oldValues.Keys.Should().NotContain("Password");
            oldValues.Keys.Should().NotContain("PasswordHash");

            var newValues = ReadJsonObject(capturedAuditLog.NewValues!);
            newValues.Should().ContainKey("fullname").WhoseValue.Should().Be("After");
            newValues.Keys.Should().NotContain("password");
            newValues.Keys.Should().NotContain("passwordhash");
        }

        [Fact]
        public async Task WriteAsync_Should_NotPersistSensitiveKeysRaw_When_RequestContainsSensitiveValues()
        {
            // Arrange
            AuditLog? capturedAuditLog = null;
            var sensitiveKeys = new[]
            {
            "password",
            "token",
            "refreshToken",
            "accessToken",
            "otp",
            "secret",
            "privateKey",
            "authorization",
            "apiKey"
        };

            var oldValues = sensitiveKeys.ToDictionary(x => x, x => (object?)$"{x}-old");
            oldValues["fullname"] = "Before";

            var newValues = sensitiveKeys.ToDictionary(x => x, x => (object?)$"{x}-new");
            newValues["fullname"] = "After";

            var auditLogRepositoryMock = new Mock<IAuditLogRepository>();
            auditLogRepositoryMock
                .Setup(x => x.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(auditLog => capturedAuditLog = auditLog);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.AuditLogs).Returns(auditLogRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

            var sut = new AuditLogWriter(unitOfWorkMock.Object, clockMock.Object, currentUserMock.Object);

            // Act
            await sut.WriteAsync(new WriteAuditLogRequest
            {
                EntityName = "User",
                EntityId = "42",
                Action = "Update",
                OldValues = oldValues,
                NewValues = newValues
            });

            // Assert
            capturedAuditLog.Should().NotBeNull();

            var persistedOldValues = ReadJsonObject(capturedAuditLog!.OldValues!);
            var persistedNewValues = ReadJsonObject(capturedAuditLog.NewValues!);

            persistedOldValues.Should().ContainKey("fullname").WhoseValue.Should().Be("Before");
            persistedNewValues.Should().ContainKey("fullname").WhoseValue.Should().Be("After");

            foreach (var sensitiveKey in sensitiveKeys)
            {
                persistedOldValues.Keys.Should().NotContain(sensitiveKey);
                persistedNewValues.Keys.Should().NotContain(sensitiveKey);
            }
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
