using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Services.Localization;
using UnifiedUserSystem.src.Contracts.DTOs.Localization;
using UnifiedUserSystem.src.Domain.Localization.Entities;

namespace UnifiedUserSystem.UnitTests.Application.Localization;

public sealed class ErrorMessageServiceTests
{
    [Fact]
    public async Task ListAsync_ShouldReturnMessagesFromRepository()
    {
        var now = new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero);
        var message = ErrorMessage.Create(
            "bad_request.title",
            "Bad request",
            "درخواست نامعتبر",
            now,
            actorUserId: null);
        var service = CreateService(
            existingMessage: message,
            out _,
            out _,
            out _);

        var result = await service.ListAsync();

        result.Should().ContainSingle();
        result[0].Key.Should().Be("bad_request.title");
        result[0].EnglishText.Should().Be("Bad request");
        result[0].PersianText.Should().Be("درخواست نامعتبر");
    }

    [Fact]
    public async Task UpsertAsync_ShouldAddMessageAndRefreshCache_WhenMessageDoesNotExist()
    {
        var service = CreateService(
            existingMessage: null,
            out var repository,
            out var unitOfWork,
            out var cache);

        var response = await service.UpsertAsync(new UpsertErrorMessageRequest
        {
            Key = "Auth.Invalid",
            EnglishText = "Invalid auth.",
            PersianText = "احراز هویت نامعتبر است.",
            IsActive = true
        });

        response.Key.Should().Be("auth.invalid");
        repository.Verify(x => x.Add(It.Is<ErrorMessage>(m => m.Key == "auth.invalid")), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertAsync_ShouldUpdateExistingMessageAndRefreshCache()
    {
        var now = new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero);
        var existing = ErrorMessage.Create(
            "server_error.detail",
            "Old text",
            "متن قدیمی",
            now,
            actorUserId: null);
        var service = CreateService(
            existing,
            out var repository,
            out var unitOfWork,
            out var cache);

        var response = await service.UpsertAsync(new UpsertErrorMessageRequest
        {
            Key = "server_error.detail",
            EnglishText = "New text",
            PersianText = "متن جدید",
            IsActive = false
        });

        response.EnglishText.Should().Be("New text");
        response.PersianText.Should().Be("متن جدید");
        response.IsActive.Should().BeFalse();
        repository.Verify(x => x.Add(It.IsAny<ErrorMessage>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ErrorMessageService CreateService(
        ErrorMessage? existingMessage,
        out Mock<IErrorMessageRepository> repository,
        out Mock<IUnitOfWork> unitOfWork,
        out Mock<ILocalizedMessageCache> cache)
    {
        repository = new Mock<IErrorMessageRepository>();
        repository
            .Setup(x => x.ListAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMessage is null
                ? Array.Empty<ErrorMessage>()
                : new[] { existingMessage });
        repository
            .Setup(x => x.FindByKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMessage);

        unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.ErrorMessages).Returns(repository.Object);
        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        cache = new Mock<ILocalizedMessageCache>();
        cache
            .Setup(x => x.RefreshAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clock = new Mock<IClock>();
        clock.SetupGet(x => x.Utcnow).Returns(new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero));

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        return new ErrorMessageService(
            unitOfWork.Object,
            cache.Object,
            clock.Object,
            currentUser.Object);
    }
}

