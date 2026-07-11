
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.src.Application.Services.Security;

public sealed class IpSecurityReportService : IIpSecurityReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public IpSecurityReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<IpSecurityEventResponse>> SearchAsync(
        IpSecurityReportQuery query,
        CancellationToken ct = default)
    {
        query ??= new IpSecurityReportQuery();

        IpSecurityEventType? eventType = null;
        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            if (!Enum.TryParse<IpSecurityEventType>(query.EventType, ignoreCase: true, out var parsed) ||
                !Enum.IsDefined(typeof(IpSecurityEventType), parsed))
            {
                throw new DomainException("IP security event type is invalid.");
            }

            eventType = parsed;
        }

        var criteria = new IpSecurityEventSearchCriteria
        {
            NormalizedIpAddress = string.IsNullOrWhiteSpace(query.IpAddress)
                ? null
                : IpAddressMatcher.NormalizeIpAddress(query.IpAddress),
            EventType = eventType,
            RuleId = query.RuleId,
            FromUtc = query.FromUtc,
            ToUtc = query.ToUtc
        };

        var events = await _unitOfWork.IpSecurityEvents.SearchAsync(criteria, ct);
        return events.Select(ToResponse).ToArray();
    }

    private static IpSecurityEventResponse ToResponse(IpSecurityEvent securityEvent)
    {
        return new IpSecurityEventResponse
        {
            Id = securityEvent.Id,
            IpAddress = securityEvent.IpAddress,
            NormalizedIpAddress = securityEvent.NormalizedIpAddress,
            EventType = securityEvent.EventType.ToString(),
            Path = securityEvent.Path,
            HttpMethod = securityEvent.HttpMethod,
            UserId = securityEvent.UserId,
            RuleId = securityEvent.RuleId,
            Reason = securityEvent.Reason,
            OccurredAt = securityEvent.OccurredAt
        };
    }
}
