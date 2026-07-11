using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Auditing;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Application.Validation;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Services.Identity
{
    public class UserCommandServiceAtomicAuditTests
    {
        [Fact]
        public async Task UpdateUserAsync_WhenAuditWriterFails_ShouldNotCallSaveChanges()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var auditLogWriterMock = new Mock<IAuditLogWriter>();
            auditLogWriterMock
                .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct))
                .ThrowsAsync(new InvalidOperationException("Audit staging failed."));

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            var act = async () => await sut.UpdateUserAsync(
                user.Id,
                new UpdateUserRequest
                {
                    FirstName = "Updated",
                    LastName = "Name"
                },
                ct);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Audit staging failed.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenSuccessful_ShouldStageAuditBeforeSaveChanges()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;
            var calls = new List<string>();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(ct))
                .Callback(() => calls.Add("save"))
                .ReturnsAsync(1);

            var auditLogWriterMock = new Mock<IAuditLogWriter>();
            auditLogWriterMock
                .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct))
                .Callback(() => calls.Add("audit"))
                .Returns(Task.CompletedTask);

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            await sut.UpdateUserAsync(
                user.Id,
                new UpdateUserRequest
                {
                    FirstName = "Updated",
                    LastName = "Name"
                },
                ct);

            calls.Should().Equal("audit", "save");
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenSuccessful_ShouldCallSaveChangesOnce()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdWithRolesAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(ct))
                .ReturnsAsync(1);

            var auditLogWriterMock = CreateSuccessfulAuditWriterMock(ct);

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            await sut.UpdateUserAsync(
                user.Id,
                new UpdateUserRequest
                {
                    FirstName = "Updated",
                    LastName = "Name"
                },
                ct);

            auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenAuditWriterFails_ShouldNotCallSaveChanges()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);

            var auditLogWriterMock = new Mock<IAuditLogWriter>();
            auditLogWriterMock
                .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct))
                .ThrowsAsync(new InvalidOperationException("Audit staging failed."));

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            var act = async () => await sut.DeactivateUserAsync(user.Id, ct);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Audit staging failed.");

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenSuccessful_ShouldStageAuditBeforeSaveChanges()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;
            var calls = new List<string>();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(ct))
                .Callback(() => calls.Add("save"))
                .ReturnsAsync(1);

            var auditLogWriterMock = new Mock<IAuditLogWriter>();
            auditLogWriterMock
                .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct))
                .Callback(() => calls.Add("audit"))
                .Returns(Task.CompletedTask);

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            await sut.DeactivateUserAsync(user.Id, ct);

            calls.Should().Equal("audit", "save");
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenSuccessful_ShouldCallSaveChangesOnce()
        {
            var now = new DateTimeOffset(2024, 01, 01, 10, 00, 00, TimeSpan.Zero);
            var actorUserId = Guid.NewGuid();
            var user = CreateUser(now, actorUserId);
            var ct = new CancellationTokenSource().Token;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.FindByIdAsync(user.Id, ct))
                .ReturnsAsync(user);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(ct))
                .ReturnsAsync(1);

            var auditLogWriterMock = CreateSuccessfulAuditWriterMock(ct);

            var sut = CreateService(
                unitOfWorkMock,
                auditLogWriterMock,
                now,
                actorUserId);

            await sut.DeactivateUserAsync(user.Id, ct);

            auditLogWriterMock.Verify(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct), Times.Once);
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        }

        private static UserCommandService CreateService(
            Mock<IUnitOfWork> unitOfWorkMock,
            Mock<IAuditLogWriter> auditLogWriterMock,
            DateTimeOffset now,
            Guid actorUserId)
        {
            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(x => x.Utcnow).Returns(now);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.SetupGet(x => x.UserId).Returns(actorUserId);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hashed-password");

            var passwordPolicyMock = new Mock<IPasswordPolicy>();

            return new UserCommandService(
                unitOfWorkMock.Object,
                clockMock.Object,
                currentUserMock.Object,
                passwordHasherMock.Object,
                passwordPolicyMock.Object,
                auditLogWriterMock.Object);
        }

        private static Mock<IAuditLogWriter> CreateSuccessfulAuditWriterMock(CancellationToken ct)
        {
            var auditLogWriterMock = new Mock<IAuditLogWriter>();
            auditLogWriterMock
                .Setup(x => x.WriteAsync(It.IsAny<WriteAuditLogRequest>(), ct))
                .Returns(Task.CompletedTask);

            return auditLogWriterMock;
        }

        private static User CreateUser(DateTimeOffset now, Guid actorUserId)
        {
            return User.CreateNew(
                "alice@example.com",
                "alice",
                "Alice",
                "Doe",
                "+989123456789",
                "initial-password-hash",
                now,
                actorUserId);
        }
    }
}