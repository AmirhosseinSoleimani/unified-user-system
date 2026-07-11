using UnifiedUserSystem.src.Application.Models;

namespace UnifiedUserSystem.Application.Abstractions.Services;

public interface IIpAccessPolicyService
{
    Task<IpAccessDecision> CheckAsync(
        string ipAddress,
        string? path,
        string? method,
        Guid? userId,
        CancellationToken ct = default);
}
