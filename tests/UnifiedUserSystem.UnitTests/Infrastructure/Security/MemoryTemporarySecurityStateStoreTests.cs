using FluentAssertions;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.UnitTests.TestHelpers;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Security;

public class MemoryTemporarySecurityStateStoreTests
{
    private readonly MutableTestClock _clock = new(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task TemporarySecurityStateStore_GetAsync_WhenKeyMissing_ShouldReturnNull()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        var result = await sut.GetStringAsync("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TemporarySecurityStateStore_SetAsync_ShouldPersistStateUntilTtlExpires()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.SetStringAsync("key", "value", TimeSpan.FromMinutes(5));

        (await sut.GetStringAsync("key")).Should().Be("value");

        _clock.Advance(TimeSpan.FromMinutes(5).Add(TimeSpan.FromTicks(1)));

        (await sut.GetStringAsync("key")).Should().BeNull();
    }

    [Fact]
    public async Task TemporarySecurityStateStore_RemoveAsync_ShouldDeleteState()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.SetStringAsync("key", "value", TimeSpan.FromMinutes(5));
        await sut.RemoveAsync("key");

        (await sut.GetStringAsync("key")).Should().BeNull();
    }

    [Fact]
    public async Task TemporarySecurityStateStore_IncrementAsync_WhenKeyMissing_ShouldStartAtOne()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        var result = await sut.IncrementAsync("counter", TimeSpan.FromMinutes(5));

        result.Should().Be(1);
        (await sut.GetStringAsync("counter")).Should().Be("1");
    }

    [Fact]
    public async Task TemporarySecurityStateStore_IncrementAsync_WhenKeyExists_ShouldIncrementValue()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.IncrementAsync("counter", TimeSpan.FromMinutes(5));
        var result = await sut.IncrementAsync("counter", TimeSpan.FromMinutes(5));

        result.Should().Be(2);
        (await sut.GetStringAsync("counter")).Should().Be("2");
    }

    [Fact]
    public async Task TemporarySecurityStateStore_IncrementAsync_ShouldRespectTtl()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.IncrementAsync("counter", TimeSpan.FromMinutes(1));

        _clock.Advance(TimeSpan.FromMinutes(1).Add(TimeSpan.FromTicks(1)));

        var result = await sut.IncrementAsync("counter", TimeSpan.FromMinutes(1));

        result.Should().Be(1);
    }

    [Fact]
    public async Task TemporarySecurityStateStore_ShouldBeSafeForDifferentKeys()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.SetStringAsync("key-1", "value-1", TimeSpan.FromMinutes(5));
        await sut.SetStringAsync("key-2", "value-2", TimeSpan.FromMinutes(5));

        (await sut.GetStringAsync("key-1")).Should().Be("value-1");
        (await sut.GetStringAsync("key-2")).Should().Be("value-2");
    }

    [Fact]
    public async Task TemporarySecurityStateStore_ShouldNotLeakStateAcrossKeys()
    {
        var sut = new MemoryTemporarySecurityStateStore(_clock);

        await sut.IncrementAsync("identity:a", TimeSpan.FromMinutes(5));
        await sut.IncrementAsync("client:a", TimeSpan.FromMinutes(5));
        await sut.IncrementAsync("client:a", TimeSpan.FromMinutes(5));

        (await sut.GetStringAsync("identity:a")).Should().Be("1");
        (await sut.GetStringAsync("client:a")).Should().Be("2");
    }
}