using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
}
