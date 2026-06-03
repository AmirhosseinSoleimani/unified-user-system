using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Api.Authorization
{
    public class OperationAuthorizationHandlerTests
    {
        [Fact]
        public async Task OperationAuthorizationHandler_WithAllowedPermission_ShouldSucceed()
        {
            var userId = Guid.NewGuid();
            var evaluator = new Mock<IPermissionEvaluator>();
            evaluator.Setup(x => x.HasPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var context = CreateContext(new OperationRequirement("users.read"), CreateAuthenticatedUser(userId.ToString()));

            await new OperationAuthorizationHandler(evaluator.Object).HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task OperationAuthorizationHandler_WithDeniedPermission_ShouldNotSucceed()
        {
            var userId = Guid.NewGuid();
            var evaluator = new Mock<IPermissionEvaluator>();
            evaluator.Setup(x => x.HasPermissionAsync(userId, "users.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var context = CreateContext(new OperationRequirement("users.read"), CreateAuthenticatedUser(userId.ToString()));

            await new OperationAuthorizationHandler(evaluator.Object).HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task OperationAuthorizationHandler_WithoutAuthenticatedUser_ShouldNotSucceed()
        {
            var evaluator = new Mock<IPermissionEvaluator>();
            var context = CreateContext(new OperationRequirement("users.read"), new ClaimsPrincipal(new ClaimsIdentity()));

            await new OperationAuthorizationHandler(evaluator.Object).HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
            evaluator.Verify(x => x.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task OperationAuthorizationHandler_WithInvalidUserIdClaim_ShouldNotSucceed()
        {
            var evaluator = new Mock<IPermissionEvaluator>();
            var context = CreateContext(new OperationRequirement("users.read"), CreateAuthenticatedUser("not-a-guid"));

            await new OperationAuthorizationHandler(evaluator.Object).HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
            evaluator.Verify(x => x.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task OperationAuthorizationHandler_ShouldCallPermissionEvaluatorWithOperationKey()
        {
            var userId = Guid.NewGuid();
            var evaluator = new Mock<IPermissionEvaluator>();
            evaluator.Setup(x => x.HasPermissionAsync(userId, "role.read", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var context = CreateContext(new OperationRequirement("role.read"), CreateAuthenticatedUser(userId.ToString()));

            await new OperationAuthorizationHandler(evaluator.Object).HandleAsync(context);

            evaluator.Verify(x => x.HasPermissionAsync(userId, "role.read", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void OperationAuthorizationHandler_ShouldNotDependOnAppDbContext()
        {
            var constructorParameterTypes = typeof(OperationAuthorizationHandler)
                .GetConstructors()
                .SelectMany(x => x.GetParameters())
                .Select(x => x.ParameterType);

            constructorParameterTypes.Should().Contain(typeof(IPermissionEvaluator));
            constructorParameterTypes.Should().NotContain(typeof(AppDbContext));
        }

        private static AuthorizationHandlerContext CreateContext(OperationRequirement requirement, ClaimsPrincipal user)
        {
            return new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);
        }

        private static ClaimsPrincipal CreateAuthenticatedUser(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) },
                authenticationType: "TestAuth"));
        }
    }
}