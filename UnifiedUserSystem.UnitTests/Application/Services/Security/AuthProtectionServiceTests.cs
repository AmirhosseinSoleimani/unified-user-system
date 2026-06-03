using FluentAssertions;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.UnitTests.TestHelpers;

namespace UnifiedUserSystem.UnitTests.Application.Services.Security
{
    public class AuthProtectionServiceTests
    {
        private readonly MutableTestClock _clock = new(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero));

        [Fact]
        public async Task AuthProtectionService_CheckAsync_WithNoState_ShouldAllow()
        {
            var sut = CreateSut();

            var result = await sut.CheckAsync("user@example.com", Client());

            result.IsAllowed.Should().BeTrue();
        }

        [Fact]
        public async Task AuthProtectionService_CheckAsync_WhenIdentityLocked_ShouldBlock()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 1, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user@example.com", Client());

            var result = await sut.CheckAsync("user@example.com", Client());

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public async Task AuthProtectionService_CheckAsync_WhenClientLocked_ShouldBlock()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 99, clientLimit: 1, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user-a@example.com", Client());

            var result = await sut.CheckAsync("user-b@example.com", Client());

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public async Task AuthProtectionService_CheckAsync_WhenCooldownActive_ShouldBlock()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 99, clientLimit: 99, cooldownSeconds: 10));

            await sut.RecordFailureAsync("user@example.com", Client());

            var result = await sut.CheckAsync("other@example.com", Client());

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public async Task AuthProtectionService_RecordFailureAsync_ShouldIncrementIdentityCounter()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync("user@example.com", Client());

            store.IncrementKeys.Should().ContainSingle(x => x.Contains(":identity:") && x.EndsWith(":failures"));
        }

        [Fact]
        public async Task AuthProtectionService_RecordFailureAsync_ShouldIncrementClientCounter()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync("user@example.com", Client());

            store.IncrementKeys.Should().ContainSingle(x => x.Contains(":client:") && x.EndsWith(":failures"));
        }

        [Fact]
        public async Task AuthProtectionService_RecordFailureAsync_WhenIdentityThresholdExceeded_ShouldLockIdentity()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store, CreateOptions(identityLimit: 1, clientLimit: 99));

            await sut.RecordFailureAsync("user@example.com", Client());

            store.SetKeys.Should().Contain(x => x.Contains(":identity:") && x.EndsWith(":lockout"));
        }

        [Fact]
        public async Task AuthProtectionService_RecordFailureAsync_WhenClientThresholdExceeded_ShouldLockClient()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store, CreateOptions(identityLimit: 99, clientLimit: 1));

            await sut.RecordFailureAsync("user@example.com", Client());

            store.SetKeys.Should().Contain(x => x.Contains(":client:") && x.EndsWith(":lockout"));
        }

        [Fact]
        public async Task AuthProtectionService_RecordFailureAsync_ShouldSetCooldownState()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store, CreateOptions(cooldownSeconds: 7));

            await sut.RecordFailureAsync("user@example.com", Client());

            store.SetKeys.Should().Contain(x => x.Contains(":client:") && x.EndsWith(":cooldown"));
        }

        [Fact]
        public async Task AuthProtectionService_ResetAsync_ShouldClearIdentityAndClientState()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.ResetAsync("user@example.com", Client());

            store.RemovedKeys.Should().HaveCount(5);
            store.RemovedKeys.Should().Contain(x => x.Contains(":identity:") && x.EndsWith(":failures"));
            store.RemovedKeys.Should().Contain(x => x.Contains(":identity:") && x.EndsWith(":lockout"));
            store.RemovedKeys.Should().Contain(x => x.Contains(":client:") && x.EndsWith(":failures"));
            store.RemovedKeys.Should().Contain(x => x.Contains(":client:") && x.EndsWith(":lockout"));
            store.RemovedKeys.Should().Contain(x => x.Contains(":client:") && x.EndsWith(":cooldown"));
        }

        [Fact]
        public async Task AuthProtectionService_ResetAsync_WhenStateMissing_ShouldNotThrow()
        {
            var sut = CreateSut();

            var act = async () => await sut.ResetAsync("missing@example.com", Client());

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseConfiguredThresholds()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 2, clientLimit: 99, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user@example.com", Client());
            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeTrue();

            await sut.RecordFailureAsync("user@example.com", Client());
            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeFalse();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseConfiguredWindowTtl()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 2, windowMinutes: 1, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user@example.com", Client());

            _clock.Advance(TimeSpan.FromMinutes(1).Add(TimeSpan.FromTicks(1)));

            await sut.RecordFailureAsync("user@example.com", Client());

            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeTrue();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseConfiguredLockoutDuration()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 1, lockoutMinutes: 1, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user@example.com", Client());
            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeFalse();

            _clock.Advance(TimeSpan.FromMinutes(1).Add(TimeSpan.FromTicks(1)));

            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeTrue();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseConfiguredCooldownDuration()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 99, clientLimit: 99, cooldownSeconds: 2));

            await sut.RecordFailureAsync("user@example.com", Client());
            (await sut.CheckAsync("other@example.com", Client())).IsAllowed.Should().BeFalse();

            _clock.Advance(TimeSpan.FromSeconds(2).Add(TimeSpan.FromTicks(1)));

            (await sut.CheckAsync("other@example.com", Client())).IsAllowed.Should().BeTrue();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseUtcClock()
        {
            var store = new MemoryTemporarySecurityStateStore(_clock);
            var sut = CreateSut(store, CreateOptions(identityLimit: 1, lockoutMinutes: 1, cooldownSeconds: 0));

            await sut.RecordFailureAsync("user@example.com", Client());

            _clock.Set(new DateTimeOffset(2026, 6, 1, 10, 1, 1, TimeSpan.Zero));

            (await sut.CheckAsync("user@example.com", Client())).IsAllowed.Should().BeTrue();
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseHashedKeys_NotRawEmailOrUsername()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync(
                "Raw.User@Example.com",
                Client(clientId: "client-raw", ip: "10.10.10.10"));

            var keys = store.IncrementKeys.Concat(store.SetKeys).ToArray();
            var joinedKeys = string.Join("|", keys).ToLowerInvariant();

            joinedKeys.Should().NotContain("raw.user");
            joinedKeys.Should().NotContain("example.com");
            joinedKeys.Should().NotContain("client-raw");
            joinedKeys.Should().NotContain("10.10.10.10");
        }

        [Fact]
        public async Task AuthProtectionService_ShouldNormalizeIdentityBeforeBuildingKeys()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync(" USER@EXAMPLE.COM ", Client());
            var firstIdentityKey = store.IncrementKeys.Single(x => x.Contains(":identity:"));

            store.IncrementKeys.Clear();

            await sut.RecordFailureAsync("user@example.com", Client());
            var secondIdentityKey = store.IncrementKeys.Single(x => x.Contains(":identity:"));

            secondIdentityKey.Should().Be(firstIdentityKey);
        }

        [Fact]
        public async Task AuthProtectionService_ShouldUseClientIdWhenProvided()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync("user@example.com", Client(clientId: "web", ip: "127.0.0.1"));
            var keyWithWebClient = store.IncrementKeys.Single(x => x.Contains(":client:"));

            store.IncrementKeys.Clear();

            await sut.RecordFailureAsync("user@example.com", Client(clientId: "mobile", ip: "127.0.0.1"));
            var keyWithMobileClient = store.IncrementKeys.Single(x => x.Contains(":client:"));

            keyWithMobileClient.Should().NotBe(keyWithWebClient);
        }

        [Fact]
        public async Task AuthProtectionService_ShouldFallbackToIpAddressWhenClientIdMissing()
        {
            var store = new SpyTemporarySecurityStateStore();
            var sut = CreateSut(store);

            await sut.RecordFailureAsync("user@example.com", Client(clientId: null, ip: "127.0.0.1"));
            var first = store.IncrementKeys.Single(x => x.Contains(":client:"));

            store.IncrementKeys.Clear();

            await sut.RecordFailureAsync("user@example.com", Client(clientId: "", ip: "127.0.0.1"));
            var second = store.IncrementKeys.Single(x => x.Contains(":client:"));

            second.Should().Be(first);
        }

        private AuthProtectionService CreateSut(
            ITemporarySecurityStateStore? store = null,
            AuthProtectionOptions? options = null)
        {
            return new AuthProtectionService(
                store ?? new MemoryTemporarySecurityStateStore(_clock),
                Microsoft.Extensions.Options.Options.Create(options ?? CreateOptions()));
        }

        private static AuthProtectionOptions CreateOptions(
            int identityLimit = 5,
            int clientLimit = 20,
            int windowMinutes = 15,
            int lockoutMinutes = 15,
            int cooldownSeconds = 2)
        {
            return new AuthProtectionOptions
            {
                MaxFailedAttemptsPerIdentity = identityLimit,
                MaxFailedAttemptsPerClient = clientLimit,
                FailedAttemptWindowMinutes = windowMinutes,
                LockoutMinutes = lockoutMinutes,
                CooldownSeconds = cooldownSeconds
            };
        }

        private static TestClientContext Client(string? clientId = "test-client", string? ip = "127.0.0.1")
        {
            return new TestClientContext
            {
                ClientId = clientId,
                IpAddress = ip
            };
        }
    }
}