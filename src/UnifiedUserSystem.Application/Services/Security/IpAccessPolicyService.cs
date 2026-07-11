
using UnifiedUserSystem.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Application.Services.Security;

public sealed class IpAccessPolicyService : IIpAccessPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public IpAccessPolicyService(IUnitOfWork unitOfWork, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<IpAccessDecision> CheckAsync(
        string ipAddress,
        string? path,
        string? method,
        Guid? userId,
        CancellationToken ct = default)
    {
        var normalizedIp = IpAddressMatcher.NormalizeIpAddress(ipAddress);
        var activeRules = await _unitOfWork.IpRules.ListActiveAsync(ct);

        var blockRule = activeRules
            .Where(x => x.RuleType == IpRuleType.Block)
            .FirstOrDefault(x => IpAddressMatcher.IsMatch(x.NormalizedIpAddressOrCidr, normalizedIp));

        if (blockRule is null)
            return IpAccessDecision.Allowed();

        var securityEvent = IpSecurityEvent.Create(
            normalizedIp,
            IpSecurityEventType.Blocked,
            path,
            method,
            userId,
            blockRule.Id,
            blockRule.Reason,
            _clock.Utcnow);

        _unitOfWork.IpSecurityEvents.Add(securityEvent);
        await _unitOfWork.SaveChangesAsync(ct);

        return IpAccessDecision.Blocked(blockRule.Id, blockRule.Reason);
    }
}