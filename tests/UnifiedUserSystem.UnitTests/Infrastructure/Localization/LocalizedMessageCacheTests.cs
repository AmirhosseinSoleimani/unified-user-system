using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Localization.Entities;
using UnifiedUserSystem.src.Infrastructure.Localization;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Localization;

public sealed class LocalizedMessageCacheTests
{
    [Fact]
    public async Task GetSnapshotAsync_ShouldBulkLoadActiveMessagesOnly()
    {
        var active = ErrorMessage.Create(
            "bad_request.title",
            "Bad request",
            "درخواست نامعتبر",
            DateTimeOffset.UtcNow,
            actorUserId: null);
        var inactive = ErrorMessage.Create(
            "server_error.title",
            "Server error",
            "خطای سرور",
            DateTimeOffset.UtcNow,
            actorUserId: null);
        inactive.Update(
            inactive.Key,
            inactive.EnglishText,
            inactive.PersianText,
            isActive: false,
            DateTimeOffset.UtcNow,
            actorUserId: null);

        var repository = new InMemoryErrorMessageRepository(new[] { active, inactive });
        var cache = CreateCache(repository);

        var snapshot = await cache.GetSnapshotAsync();

        snapshot.Messages.Should().ContainKey("bad_request.title");
        snapshot.Messages.Should().NotContainKey("server_error.title");
        repository.ListCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_ShouldReadFromMemoryAfterFirstLoad()
    {
        var message = ErrorMessage.Create(
            "bad_request.title",
            "Bad request",
            "درخواست نامعتبر",
            DateTimeOffset.UtcNow,
            actorUserId: null);
        var repository = new InMemoryErrorMessageRepository(new[] { message });
        var cache = CreateCache(repository);

        var first = await cache.GetAsync(
            "bad_request.title",
            new LocalizedMessage("Fallback", "پیش فرض"));
        var second = await cache.GetAsync(
            "bad_request.title",
            new LocalizedMessage("Fallback", "پیش فرض"));

        first.Persian.Should().Be("درخواست نامعتبر");
        second.English.Should().Be("Bad request");
        repository.ListCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReloadMessages()
    {
        var repository = new InMemoryErrorMessageRepository(new[]
        {
            ErrorMessage.Create(
                "bad_request.title",
                "Bad request",
                "درخواست نامعتبر",
                DateTimeOffset.UtcNow,
                actorUserId: null)
        });
        var cache = CreateCache(repository);
        _ = await cache.GetSnapshotAsync();

        repository.ReplaceMessages(new[]
        {
            ErrorMessage.Create(
                "bad_request.title",
                "Bad request updated",
                "درخواست نامعتبر جدید",
                DateTimeOffset.UtcNow.AddMinutes(1),
                actorUserId: null)
        });

        await cache.RefreshAsync();
        var updated = await cache.GetAsync(
            "bad_request.title",
            new LocalizedMessage("Fallback", "پیش فرض"));

        updated.English.Should().Be("Bad request updated");
        updated.Persian.Should().Be("درخواست نامعتبر جدید");
        repository.ListCallCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAsync_ShouldUseFallback_WhenKeyDoesNotExist()
    {
        var repository = new InMemoryErrorMessageRepository(Array.Empty<ErrorMessage>());
        var cache = CreateCache(repository);

        var result = await cache.GetAsync(
            "missing.key",
            new LocalizedMessage("Fallback", "پیش فرض"));

        result.English.Should().Be("Fallback");
        result.Persian.Should().Be("پیش فرض");
    }

    private static LocalizedMessageCache CreateCache(IErrorMessageRepository repository)
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton(repository);

        var provider = services.BuildServiceProvider();

        return new LocalizedMessageCache(
            provider.GetRequiredService<IMemoryCache>(),
            provider.GetRequiredService<IServiceScopeFactory>());
    }

    private sealed class InMemoryErrorMessageRepository : IErrorMessageRepository
    {
        private IReadOnlyList<ErrorMessage> _messages;

        public InMemoryErrorMessageRepository(IReadOnlyList<ErrorMessage> messages)
        {
            _messages = messages;
        }

        public int ListCallCount { get; private set; }

        public Task<IReadOnlyList<ErrorMessage>> ListAsync(
            bool activeOnly = false,
            CancellationToken ct = default)
        {
            ListCallCount++;

            var messages = activeOnly
                ? _messages.Where(x => x.IsActive).ToArray()
                : _messages.ToArray();

            return Task.FromResult<IReadOnlyList<ErrorMessage>>(messages);
        }

        public Task<ErrorMessage?> FindByKeyAsync(string key, CancellationToken ct = default)
        {
            return Task.FromResult(_messages.FirstOrDefault(x => x.Key == key));
        }

        public void Add(ErrorMessage message)
        {
            _messages = _messages.Concat(new[] { message }).ToArray();
        }

        public void ReplaceMessages(IReadOnlyList<ErrorMessage> messages)
        {
            _messages = messages;
        }
    }
}