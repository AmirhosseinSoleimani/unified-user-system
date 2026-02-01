using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

            var roleNames = context.User.FindAll(ClaimTypes.Role)
                .Select(x => (x.Value ?? "").Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            if (roleNames.Length == 0) return;

            var opId = await _db.Operation
                .Where(o => o.IsActive && o.Key == requirement.OperationKey)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();

            if(opId == Guid.Empty) return;

            var roleIds = await _db.Roles
                .Where(r => r.IsActive && roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            if (roleIds.Count == 0) return;

            var allowed = await _db.RoleOperations
                .AnyAsync(ro => roleIds.Contains(ro.RoleId) && ro.OperationId == opId);

            if (allowed)
                context.Succeed(requirement);
        }
    }
}
