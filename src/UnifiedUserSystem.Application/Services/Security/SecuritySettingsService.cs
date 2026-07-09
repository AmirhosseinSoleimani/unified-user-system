using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Domain.Common;
using Microsoft.Extensions.Options;

namespace UnifiedUserSystem.Application.Services.Security;

public sealed class SecuritySettingsService : ISecuritySettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly SecuritySettingsDefaultsOptions _defaults;

    public SecuritySettingsService(
       IUnitOfWork unitOfWork,
       IClock clock,
       ICurrentUser currentUser,
       IOptions<SecuritySettingsDefaultsOptions> defaults)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
        _defaults = defaults.Value;
    }

    public async Task<SecuritySettingsResponse> GetEffectiveAsync(CancellationToken ct = default)
    {
        var settings = await _unitOfWork.SecuritySettings.GetAsync(ct);

        if (settings is not null)
            return ToResponse(settings);

        var defaults = BuildDefaultSettings();
        return ToResponse(defaults);
    }

    public async Task<SecuritySettingsResponse> UpdateAsync(
        UpdateSecuritySettingsRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        var settings = await _unitOfWork.SecuritySettings.GetAsync(ct);
        var now = _clock.Utcnow;
        var actorUserId = _currentUser.UserId;

        if (settings is null)
        {
            settings = SecuritySettings.Create(
                request.IsMfaEnabled,
                request.IsOtpEnabled,
                request.OtpExpirationMinutes,
                request.OtpMaxAttempts,
                request.LoginRateLimitPermitLimit,
                request.LoginRateLimitWindowSeconds,
                request.RefreshTokenRateLimitPermitLimit,
                request.RefreshTokenRateLimitWindowSeconds,
                request.AllowedIpRanges,
                request.BlockedIpRanges,
                now,
                actorUserId);

            _unitOfWork.SecuritySettings.Add(settings);
        }
        else
        {
            settings.Update(
                request.IsMfaEnabled,
                request.IsOtpEnabled,
                request.OtpExpirationMinutes,
                request.OtpMaxAttempts,
                request.LoginRateLimitPermitLimit,
                request.LoginRateLimitWindowSeconds,
                request.RefreshTokenRateLimitPermitLimit,
                request.RefreshTokenRateLimitWindowSeconds,
                request.AllowedIpRanges,
                request.BlockedIpRanges,
                now,
                actorUserId);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(settings);
    }

    private SecuritySettings BuildDefaultSettings()
    {
        return SecuritySettings.Create(
            _defaults.IsMfaEnabled,
            _defaults.IsOtpEnabled,
            _defaults.OtpExpirationMinutes,
            _defaults.OtpMaxAttempts,
            _defaults.LoginRateLimitPermitLimit,
            _defaults.LoginRateLimitWindowSeconds,
            _defaults.RefreshTokenRateLimitPermitLimit,
            _defaults.RefreshTokenRateLimitWindowSeconds,
            _defaults.AllowedIpRanges,
            _defaults.BlockedIpRanges,
            _clock.Utcnow,
            _currentUser.UserId);
    }

    private static SecuritySettingsResponse ToResponse(SecuritySettings settings)
    {
        return new SecuritySettingsResponse
        {
            Id = settings.Id,
            IsMfaEnabled = settings.IsMfaEnabled,
            IsOtpEnabled = settings.IsOtpEnabled,
            OtpExpirationMinutes = settings.OtpExpirationMinutes,
            OtpMaxAttempts = settings.OtpMaxAttempts,
            LoginRateLimitPermitLimit = settings.LoginRateLimitPermitLimit,
            LoginRateLimitWindowSeconds = settings.LoginRateLimitWindowSeconds,
            RefreshTokenRateLimitPermitLimit = settings.RefreshTokenRateLimitPermitLimit,
            RefreshTokenRateLimitWindowSeconds = settings.RefreshTokenRateLimitWindowSeconds,
            AllowedIpRanges = settings.GetAllowedIpRanges(),
            BlockedIpRanges = settings.GetBlockedIpRanges(),
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }
}
