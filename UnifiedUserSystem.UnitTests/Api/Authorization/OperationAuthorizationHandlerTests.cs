using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Application.Interfaces.Security;

using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;

namespace UnifiedUserSystem.UnitTests.Api.Authorization
{
    public class OperationAuthorizationHandlerTests
    {
        private static AuthorizationHandlerContext CreateContext(
            OperationRequirement requirement,
            ClaimsPrincipal user)
        {
            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                resource: null);
        }

        private static ClaimsPrincipal CreateAuthenticatedUser(string? subClaim)
        {
            var claims = new List<Claim>();

            if (subClaim is not null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subClaim));

            var identity = new ClaimsIdentity(claims, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task Should_Fail_When_User_Is_Anonymous()
        {
            var permissionEvaluator = new Mock<IPermissionEvaluator>();

            var handler = new OperationAuthorizationHandler(permissionEvaluator.Object);
            var requirement = new OperationRequirement("operation.test");

            var context = CreateContext(
                requirement,
                new ClaimsPrincipal(new ClaimsIdentity()));

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();

            permissionEvaluator.Verify(
                x => x.HasPermissionAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Fail_When_Sub_Claim_Is_Invalid()
        {
            var permissionEvaluator = new Mock<IPermissionEvaluator>();

            var handler = new OperationAuthorizationHandler(permissionEvaluator.Object);
            var requirement = new OperationRequirement("operation.test");

            var context = CreateContext(
                requirement,
                CreateAuthenticatedUser("invalid-guid"));

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();

            permissionEvaluator.Verify(
                x => x.HasPermissionAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Fail_When_User_Does_Not_Have_Operation()
        {
            var userId = Guid.NewGuid();

            var permissionEvaluator = new Mock<IPermissionEvaluator>();
            permissionEvaluator
                .Setup(x => x.HasPermissionAsync(
                    userId,
                    "operation.test",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = new OperationAuthorizationHandler(permissionEvaluator.Object);
            var requirement = new OperationRequirement("operation.test");

            var context = CreateContext(
                requirement,
                CreateAuthenticatedUser(userId.ToString()));

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();

            permissionEvaluator.Verify(
                x => x.HasPermissionAsync(
                    userId,
                    "operation.test",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Succeed_When_User_Has_Operation()
        {
            var userId = Guid.NewGuid();

            var permissionEvaluator = new Mock<IPermissionEvaluator>();
            permissionEvaluator
                .Setup(x => x.HasPermissionAsync(
                    userId,
                    "operation.test",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new OperationAuthorizationHandler(permissionEvaluator.Object);
            var requirement = new OperationRequirement("operation.test");

            var context = CreateContext(
                requirement,
                CreateAuthenticatedUser(userId.ToString()));

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();

            permissionEvaluator.Verify(
                x => x.HasPermissionAsync(
                    userId,
                    "operation.test",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Call_PermissionEvaluator_With_OperationKey()
        {
            var userId = Guid.NewGuid();

            var permissionEvaluator = new Mock<IPermissionEvaluator>();
            permissionEvaluator
                .Setup(x => x.HasPermissionAsync(
                    userId,
                    "users.read",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new OperationAuthorizationHandler(permissionEvaluator.Object);
            var requirement = new OperationRequirement("users.read");

            var context = CreateContext(
                requirement,
                CreateAuthenticatedUser(userId.ToString()));

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();

            permissionEvaluator.Verify(
                x => x.HasPermissionAsync(
                    userId,
                    "users.read",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}