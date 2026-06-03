using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnifiedUserSystem.src.Application.Interfaces.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var principal = GetPrincipal();

                if (principal is null)
                    return null;

                var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub")
                    ?? principal.FindFirstValue("uid")
                    ?? principal.FindFirstValue("userId");

                return Guid.TryParse(value, out var userId)
                    ? userId
                    : null;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                var principal = GetPrincipal();

                return principal?.Identity?.IsAuthenticated == true;
            }
        }

        private ClaimsPrincipal? GetPrincipal()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
    }
}