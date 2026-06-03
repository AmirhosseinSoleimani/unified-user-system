using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Services.Security;

namespace UnifiedUserSystem.UnitTests.Application.Services.Security
{
    public class PermissionCacheInvalidatorTests
    {
        [Fact]
        public async Task InvalidateForUserAsync_ShouldInvalidateUserPermissionCache()
        {
            var userId = Guid.NewGuid();
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            var sut = new PermissionCacheInvalidator(cache.Object, repository.Object);

            await sut.InvalidateForUserAsync(userId);

            cache.Verify(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InvalidateForRoleAsync_ShouldInvalidateAffectedUsersPermissionCache()
        {
            var userOne = Guid.NewGuid();
            var userTwo = Guid.NewGuid();

            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();
            repository.Setup(x => x.ListUserIdsInRoleAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { userOne, userTwo });

            var sut = new PermissionCacheInvalidator(cache.Object, repository.Object);

            await sut.InvalidateForRoleAsync(7);

            cache.Verify(x => x.InvalidateUserAsync(userOne, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(x => x.InvalidateUserAsync(userTwo, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InvalidateForOperationAsync_ShouldInvalidateOperationPermissionCache()
        {
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            var sut = new PermissionCacheInvalidator(cache.Object, repository.Object);

            await sut.InvalidateForOperationAsync("OP:Users.Read");

            cache.Verify(x => x.InvalidateOperationAsync("users.read", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ClearAsync_ShouldClearPermissionCache()
        {
            var cache = new Mock<IPermissionCache>();
            var repository = new Mock<IPermissionReadRepository>();

            var sut = new PermissionCacheInvalidator(cache.Object, repository.Object);

            await sut.ClearAsync();

            cache.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}