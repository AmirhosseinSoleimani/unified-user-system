using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using UnifiedUserSystem.src.Infrastructure.Security;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Security
{
    public class MemoryPermissionCacheTests
    {
        [Fact]
        public async Task MemoryPermissionCache_GetPermissionAsync_WhenMissing_ShouldReturnNull()
        {
            var sut = CreateSut();

            var result = await sut.GetPermissionAsync(Guid.NewGuid(), "users.read");

            result.Should().BeNull();
        }

        [Fact]
        public async Task MemoryPermissionCache_SetPermissionAsync_ShouldStoreAllowedValue()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();

            await sut.SetPermissionAsync(userId, "users.read", true, TimeSpan.FromMinutes(1));

            var result = await sut.GetPermissionAsync(userId, "users.read");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MemoryPermissionCache_SetPermissionAsync_ShouldStoreDeniedValue()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();

            await sut.SetPermissionAsync(userId, "users.read", false, TimeSpan.FromMinutes(1));

            var result = await sut.GetPermissionAsync(userId, "users.read");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MemoryPermissionCache_InvalidateUserAsync_ShouldRemoveOnlyThatUserEntries()
        {
            var sut = CreateSut();
            var userOne = Guid.NewGuid();
            var userTwo = Guid.NewGuid();

            await sut.SetPermissionAsync(userOne, "users.read", true, TimeSpan.FromMinutes(1));
            await sut.SetPermissionAsync(userTwo, "users.read", true, TimeSpan.FromMinutes(1));

            await sut.InvalidateUserAsync(userOne);

            (await sut.GetPermissionAsync(userOne, "users.read")).Should().BeNull();
            (await sut.GetPermissionAsync(userTwo, "users.read")).Should().BeTrue();
        }

        [Fact]
        public async Task MemoryPermissionCache_InvalidateOperationAsync_ShouldRemoveOnlyThatOperationEntries()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();

            await sut.SetPermissionAsync(userId, "users.read", true, TimeSpan.FromMinutes(1));
            await sut.SetPermissionAsync(userId, "role.read", false, TimeSpan.FromMinutes(1));

            await sut.InvalidateOperationAsync("users.read");

            (await sut.GetPermissionAsync(userId, "users.read")).Should().BeNull();
            (await sut.GetPermissionAsync(userId, "role.read")).Should().BeFalse();
        }

        [Fact]
        public async Task MemoryPermissionCache_ShouldNotLeakEntriesBetweenUsers()
        {
            var sut = CreateSut();
            var userOne = Guid.NewGuid();
            var userTwo = Guid.NewGuid();

            await sut.SetPermissionAsync(userOne, "users.read", true, TimeSpan.FromMinutes(1));
            await sut.SetPermissionAsync(userTwo, "users.read", false, TimeSpan.FromMinutes(1));

            (await sut.GetPermissionAsync(userOne, "users.read")).Should().BeTrue();
            (await sut.GetPermissionAsync(userTwo, "users.read")).Should().BeFalse();
        }

        [Fact]
        public async Task MemoryPermissionCache_ShouldNotLeakEntriesBetweenOperations()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();

            await sut.SetPermissionAsync(userId, "users.read", true, TimeSpan.FromMinutes(1));
            await sut.SetPermissionAsync(userId, "role.read", false, TimeSpan.FromMinutes(1));

            (await sut.GetPermissionAsync(userId, "users.read")).Should().BeTrue();
            (await sut.GetPermissionAsync(userId, "role.read")).Should().BeFalse();
        }

        private static MemoryPermissionCache CreateSut()
        {
            return new MemoryPermissionCache(new MemoryCache(new MemoryCacheOptions()));
        }
    }
}