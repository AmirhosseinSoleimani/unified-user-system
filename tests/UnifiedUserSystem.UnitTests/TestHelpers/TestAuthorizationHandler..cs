using Microsoft.AspNetCore.Authorization;

namespace UnifiedUserSystem.UnitTests.TestHelpers
{
    public sealed class TestAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToArray())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}