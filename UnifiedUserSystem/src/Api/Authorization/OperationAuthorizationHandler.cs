using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public class OperationAuthorizationHandler : AuthorizationHandler<OperationRequirement>
    {
        private readonly AppDbContext _db;

        public OperationAuthorizationHandler(AppDbContext db)
        {
            _db = db;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationRequirement requirement
            )
        {
            if (context.User?.Identity?.IsAuthenticated != true) return;

            var sub = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(sub, out var userId)) return;

            var allowed = await _db.UserRoles.AnyAsync(ur =>
                ur.UserId == userId &&
                ur.User.IsActive &&
                ur.Role.IsActive &&
                ur.Role.RoleOperations.Any(ro =>
                ro.Operation.IsActive && ro.Operation.Key == requirement.OperationKey)
            );

            if (allowed)
                context.Succeed(requirement);
        }
    }
}
