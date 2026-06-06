using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application.Services;

public class OperationServiceCorrectnessTests
{
    private static readonly DateTimeOffset Now = new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
    private readonly Guid _actorUserId = Guid.NewGuid();

    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
    private readonly Mock<IOperationRepository> _operations = new(MockBehavior.Strict);
    private readonly Mock<IClock> _clock = new(MockBehavior.Strict);
    private readonly Mock<ICurrentUser> _currentUser = new(MockBehavior.Strict);
    private readonly Mock<IPermissionCacheInvalidator> _permissionCacheInvalidator = new(MockBehavior.Strict);

    [Fact]
    public async Task OperationService_ActivateOperationAsync_ShouldActivateOperation()
    {
        var operation = Operation.Create("users.read", "Read Users", Now, _actorUserId);
        operation.Deactivate(Now.AddMinutes(1), _actorUserId);
        var sut = CreateSut(operation);

        await sut.ActivateOperationAsync(operation.Id);

        operation.IsActive.Should().BeTrue();
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _permissionCacheInvalidator.Verify(
            x => x.InvalidateForOperationAsync("users.read", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OperationService_DeactivateOperationAsync_ShouldDeactivateOperation()
    {
        var operation = Operation.Create("users.read", "Read Users", Now, _actorUserId);
        var sut = CreateSut(operation);

        await sut.DeactivateOperationAsync(operation.Id);

        operation.IsActive.Should().BeFalse();
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _permissionCacheInvalidator.Verify(
            x => x.InvalidateForOperationAsync("users.read", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OperationService_ActivateOperationAsync_WithMissingOperation_ShouldReturnNotFoundOrThrowBasedOnContract()
    {
        var sut = CreateSut(operation: null);

        var act = () => sut.ActivateOperationAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Operation not found*");

        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OperationService_DeactivateOperationAsync_WithMissingOperation_ShouldReturnNotFoundOrThrowBasedOnContract()
    {
        var sut = CreateSut(operation: null);

        var act = () => sut.DeactivateOperationAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Operation not found*");

        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private OperationService CreateSut(Operation? operation)
    {
        _operations.Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        _uow.SetupGet(x => x.Operations).Returns(_operations.Object);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _clock.SetupGet(x => x.Utcnow).Returns(Now);
        _currentUser.SetupGet(x => x.UserId).Returns(_actorUserId);

        _permissionCacheInvalidator
            .Setup(x => x.InvalidateForOperationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new OperationService(
            _uow.Object,
            _clock.Object,
            _currentUser.Object,
            _permissionCacheInvalidator.Object);
    }
}