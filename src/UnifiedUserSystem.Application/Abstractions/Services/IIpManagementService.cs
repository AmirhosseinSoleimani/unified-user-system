
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IIpManagementService
{
    Task<IpRuleResponse> CreateRuleAsync(CreateIpRuleRequest request, CancellationToken ct = default);
    Task<IpRuleResponse> UpdateRuleAsync(Guid id, UpdateIpRuleRequest request, CancellationToken ct = default);
    Task DisableRuleAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<IpRuleResponse>> ListRulesAsync(CancellationToken ct = default);
}
