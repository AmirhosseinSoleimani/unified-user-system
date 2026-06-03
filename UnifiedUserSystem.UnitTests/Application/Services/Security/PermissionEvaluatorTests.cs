using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Services.Security;

namespace UnifiedUserSystem.UnitTests.Application.Services.Security
{
    public class PermissionEvaluatorTests
    {
        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WhenCacheAllows_ShouldReturnTrueWithoutRepositoryCall()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            cache.Setup(x => x.GetPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(userId, "users.read");

            result.Should().BeTrue();
            repository.Verify(x => x.UserHasOperationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WhenCacheDenies_ShouldReturnFalseWithoutRepositoryCall()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            cache.Setup(x => x.GetPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(userId, "users.read");

            result.Should().BeFalse();
            repository.Verify(x => x.UserHasOperationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WhenCacheMissAndRepositoryAllows_ShouldReturnTrueAndCacheResult()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            cache.Setup(x => x.GetPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync((bool?)null);

            repository.Setup(x => x.UserHasOperationAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(userId, "OP:Users.Read");

            result.Should().BeTrue();
            repository.Verify(x => x.UserHasOperationAsync(userId, "users.read", It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(x => x.SetPermissionAsync(userId, "users.read", true, TimeSpan.FromSeconds(60), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WhenCacheMissAndRepositoryDenies_ShouldReturnFalseAndCacheResult()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            cache.Setup(x => x.GetPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync((bool?)null);

            repository.Setup(x => x.UserHasOperationAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(userId, "users.read");

            result.Should().BeFalse();
            cache.Verify(x => x.SetPermissionAsync(userId, "users.read", false, TimeSpan.FromSeconds(60), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WithEmptyOperationKey_ShouldReturnFalseOrThrowBasedOnContract()
        {
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();
            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(Guid.NewGuid(), " ");

            result.Should().BeFalse();
            cache.Verify(x => x.GetPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(x => x.UserHasOperationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_WithEmptyUserId_ShouldReturnFalseOrThrowBasedOnContract()
        {
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();
            var sut = CreateSut(cache.Object, repository.Object);

            var result = await sut.HasPermissionAsync(Guid.Empty, "users.read");

            result.Should().BeFalse();
            cache.Verify(x => x.GetPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(x => x.UserHasOperationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PermissionEvaluator_HasPermissionAsync_ShouldUseConfiguredCacheTtl()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            cache.Setup(x => x.GetPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync((bool?)null);

            repository.Setup(x => x.UserHasOperationAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut(cache.Object, repository.Object, cacheTtlSeconds: 123);

            await sut.HasPermissionAsync(userId, "users.read");

            cache.Verify(x => x.SetPermissionAsync(userId, "users.read", true, TimeSpan.FromSeconds(123), It.IsAny<CancellationToken>()), Times.Once);
        }

        private static PermissionEvaluator CreateSut(
            IPermissionCache cache,
            IPermissionReadRepository repository,
            int cacheTtlSeconds = 60)
        {
            return new PermissionEvaluator(
                cache,
                repository,
                Microsoft.Extensions.Options.Options.Create(new PermissionEvaluationOptions { CacheTtlSeconds = cacheTtlSeconds }));
        }
    }
}