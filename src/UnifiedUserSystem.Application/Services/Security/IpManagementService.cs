
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Application.Services.Security;

public sealed class IpManagementService : IIpManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public IpManagementService(
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IpRuleResponse> CreateRuleAsync(CreateIpRuleRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        var ruleType = ParseRuleType(request.RuleType);
        var rule = IpRule.Create(
            request.IpAddressOrCidr,
            ruleType,
            request.Reason,
            _clock.Utcnow,
            _currentUser.UserId);

        _unitOfWork.IpRules.Add(rule);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rule);
    }

    public async Task<IpRuleResponse> UpdateRuleAsync(Guid id, UpdateIpRuleRequest request, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new DomainException("IP rule id is required.");

        if (request is null)
            throw new DomainException("Request is null.");

        var rule = await _unitOfWork.IpRules.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("IP rule not found.");

        rule.Update(
            request.IpAddressOrCidr,
            ParseRuleType(request.RuleType),
            request.Reason,
            request.IsActive,
            _clock.Utcnow,
            _currentUser.UserId);

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(rule);
    }

    public async Task DisableRuleAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new DomainException("IP rule id is required.");

        var rule = await _unitOfWork.IpRules.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("IP rule not found.");

        rule.Disable(_clock.Utcnow, _currentUser.UserId);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IpRuleResponse>> ListRulesAsync(CancellationToken ct = default)
    {
        var rules = await _unitOfWork.IpRules.ListAsync(ct);
        return rules.Select(ToResponse).ToArray();
    }

    private static IpRuleType ParseRuleType(string? ruleType)
    {
        if (!Enum.TryParse<IpRuleType>(ruleType, ignoreCase: true, out var parsed) ||
            !Enum.IsDefined(typeof(IpRuleType), parsed))
        {
            throw new DomainException("IP rule type is invalid.");
        }

        return parsed;
    }

    private static IpRuleResponse ToResponse(IpRule rule)
    {
        return new IpRuleResponse
        {
            Id = rule.Id,
            IpAddressOrCidr = rule.IpAddressOrCidr,
            NormalizedIpAddressOrCidr = rule.NormalizedIpAddressOrCidr,
            RuleType = rule.RuleType.ToString(),
            Reason = rule.Reason,
            IsActive = rule.IsActive,
            DisabledAt = rule.DisabledAt,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
