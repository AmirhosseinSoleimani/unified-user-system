using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public sealed class OperationAuthorizationHandler : AuthorizationHandler<OperationRequirement>
    {
        private readonly IPermissionEvaluator _permissionEvaluator;

        public OperationAuthorizationHandler(IPermissionEvaluator permissionEvaluator)
        {
            _permissionEvaluator = permissionEvaluator;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var userIdValue =
                context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                context.User.FindFirstValue("sub") ??
                context.User.FindFirstValue("uid") ??
                context.User.FindFirstValue("userId");

            if (!Guid.TryParse(userIdValue, out var userId))
                return;

            var allowed = await _permissionEvaluator.HasPermissionAsync(
                userId,
                requirement.OperationKey);

            if (allowed)
                context.Succeed(requirement);
        }
    }
}