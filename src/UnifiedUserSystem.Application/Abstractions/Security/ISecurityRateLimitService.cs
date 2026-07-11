
using UnifiedUserSystem.src.Application.Models;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface ISecurityRateLimitService
{
    Task<RateLimitDecision> CheckAsync(
        SecurityRateLimitContext context,
        CancellationToken cancellationToken = default);
}
