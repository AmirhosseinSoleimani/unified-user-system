using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(User user);
    }
}
