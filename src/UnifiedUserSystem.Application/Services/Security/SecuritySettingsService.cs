using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Application.Services.Security;

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
                isMfaEnabled: request.IsMfaEnabled,
                isOtpEnabled: request.IsOtpEnabled,
                isEmailOtpEnabled: request.IsEmailOtpEnabled,
                isPhoneOtpEnabled: request.IsPhoneOtpEnabled,
                otpExpirationMinutes: request.OtpExpirationMinutes,
                otpMaxAttempts: request.OtpMaxAttempts,
                loginRateLimitPermitLimit: request.LoginRateLimitPermitLimit,
                loginRateLimitWindowSeconds: request.LoginRateLimitWindowSeconds,
                loginRateLimitQueueLimit: request.LoginRateLimitQueueLimit,
                loginRateLimitCooldownSeconds: request.LoginRateLimitCooldownSeconds,
                loginLockoutFailureThreshold: request.LoginLockoutFailureThreshold,
                loginLockoutDurationSeconds: request.LoginLockoutDurationSeconds,
                refreshTokenRateLimitPermitLimit: request.RefreshTokenRateLimitPermitLimit,
                refreshTokenRateLimitWindowSeconds: request.RefreshTokenRateLimitWindowSeconds,
                refreshTokenRateLimitQueueLimit: request.RefreshTokenRateLimitQueueLimit,
                refreshTokenRateLimitCooldownSeconds: request.RefreshTokenRateLimitCooldownSeconds,
                sensitiveAdminRateLimitPermitLimit: request.SensitiveAdminRateLimitPermitLimit,
                sensitiveAdminRateLimitWindowSeconds: request.SensitiveAdminRateLimitWindowSeconds,
                sensitiveAdminRateLimitQueueLimit: request.SensitiveAdminRateLimitQueueLimit,
                sensitiveAdminRateLimitCooldownSeconds: request.SensitiveAdminRateLimitCooldownSeconds,
                allowedIpRanges: request.AllowedIpRanges,
                blockedIpRanges: request.BlockedIpRanges,
                nowUtc: now,
                actorUserId: actorUserId);

            _unitOfWork.SecuritySettings.Add(settings);
        }
        else
        {
            settings.Update(
                isMfaEnabled: request.IsMfaEnabled,
                isOtpEnabled: request.IsOtpEnabled,
                isEmailOtpEnabled: request.IsEmailOtpEnabled,
                isPhoneOtpEnabled: request.IsPhoneOtpEnabled,
                otpExpirationMinutes: request.OtpExpirationMinutes,
                otpMaxAttempts: request.OtpMaxAttempts,
                loginRateLimitPermitLimit: request.LoginRateLimitPermitLimit,
                loginRateLimitWindowSeconds: request.LoginRateLimitWindowSeconds,
                loginRateLimitQueueLimit: request.LoginRateLimitQueueLimit,
                loginRateLimitCooldownSeconds: request.LoginRateLimitCooldownSeconds,
                loginLockoutFailureThreshold: request.LoginLockoutFailureThreshold,
                loginLockoutDurationSeconds: request.LoginLockoutDurationSeconds,
                refreshTokenRateLimitPermitLimit: request.RefreshTokenRateLimitPermitLimit,
                refreshTokenRateLimitWindowSeconds: request.RefreshTokenRateLimitWindowSeconds,
                refreshTokenRateLimitQueueLimit: request.RefreshTokenRateLimitQueueLimit,
                refreshTokenRateLimitCooldownSeconds: request.RefreshTokenRateLimitCooldownSeconds,
                sensitiveAdminRateLimitPermitLimit: request.SensitiveAdminRateLimitPermitLimit,
                sensitiveAdminRateLimitWindowSeconds: request.SensitiveAdminRateLimitWindowSeconds,
                sensitiveAdminRateLimitQueueLimit: request.SensitiveAdminRateLimitQueueLimit,
                sensitiveAdminRateLimitCooldownSeconds: request.SensitiveAdminRateLimitCooldownSeconds,
                allowedIpRanges: request.AllowedIpRanges,
                blockedIpRanges: request.BlockedIpRanges,
                nowUtc: now,
                actorUserId: actorUserId);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(settings);
    }

    private SecuritySettings BuildDefaultSettings()
    {
        return SecuritySettings.Create(
            isMfaEnabled: _defaults.IsMfaEnabled,
            isOtpEnabled: _defaults.IsOtpEnabled,
            isEmailOtpEnabled: _defaults.IsEmailOtpEnabled,
            isPhoneOtpEnabled: _defaults.IsPhoneOtpEnabled,
            otpExpirationMinutes: _defaults.OtpExpirationMinutes,
            otpMaxAttempts: _defaults.OtpMaxAttempts,
            loginRateLimitPermitLimit: _defaults.LoginRateLimitPermitLimit,
            loginRateLimitWindowSeconds: _defaults.LoginRateLimitWindowSeconds,
            loginRateLimitQueueLimit: _defaults.LoginRateLimitQueueLimit,
            loginRateLimitCooldownSeconds: _defaults.LoginRateLimitCooldownSeconds,
            loginLockoutFailureThreshold: _defaults.LoginLockoutFailureThreshold,
            loginLockoutDurationSeconds: _defaults.LoginLockoutDurationSeconds,
            refreshTokenRateLimitPermitLimit: _defaults.RefreshTokenRateLimitPermitLimit,
            refreshTokenRateLimitWindowSeconds: _defaults.RefreshTokenRateLimitWindowSeconds,
            refreshTokenRateLimitQueueLimit: _defaults.RefreshTokenRateLimitQueueLimit,
            refreshTokenRateLimitCooldownSeconds: _defaults.RefreshTokenRateLimitCooldownSeconds,
            sensitiveAdminRateLimitPermitLimit: _defaults.SensitiveAdminRateLimitPermitLimit,
            sensitiveAdminRateLimitWindowSeconds: _defaults.SensitiveAdminRateLimitWindowSeconds,
            sensitiveAdminRateLimitQueueLimit: _defaults.SensitiveAdminRateLimitQueueLimit,
            sensitiveAdminRateLimitCooldownSeconds: _defaults.SensitiveAdminRateLimitCooldownSeconds,
            allowedIpRanges: _defaults.AllowedIpRanges,
            blockedIpRanges: _defaults.BlockedIpRanges,
            nowUtc: _clock.Utcnow,
            actorUserId: _currentUser.UserId);
    }

    private static SecuritySettingsResponse ToResponse(SecuritySettings settings)
    {
        return new SecuritySettingsResponse
        {
            Id = settings.Id,
            IsMfaEnabled = settings.IsMfaEnabled,
            IsOtpEnabled = settings.IsOtpEnabled,
            IsEmailOtpEnabled = settings.IsEmailOtpEnabled,
            IsPhoneOtpEnabled = settings.IsPhoneOtpEnabled,
            OtpExpirationMinutes = settings.OtpExpirationMinutes,
            OtpMaxAttempts = settings.OtpMaxAttempts,
            LoginRateLimitPermitLimit = settings.LoginRateLimitPermitLimit,
            LoginRateLimitWindowSeconds = settings.LoginRateLimitWindowSeconds,
            LoginRateLimitQueueLimit = settings.LoginRateLimitQueueLimit,
            LoginRateLimitCooldownSeconds = settings.LoginRateLimitCooldownSeconds,
            LoginLockoutFailureThreshold = settings.LoginLockoutFailureThreshold,
            LoginLockoutDurationSeconds = settings.LoginLockoutDurationSeconds,
            RefreshTokenRateLimitPermitLimit = settings.RefreshTokenRateLimitPermitLimit,
            RefreshTokenRateLimitWindowSeconds = settings.RefreshTokenRateLimitWindowSeconds,
            RefreshTokenRateLimitQueueLimit = settings.RefreshTokenRateLimitQueueLimit,
            RefreshTokenRateLimitCooldownSeconds = settings.RefreshTokenRateLimitCooldownSeconds,
            SensitiveAdminRateLimitPermitLimit = settings.SensitiveAdminRateLimitPermitLimit,
            SensitiveAdminRateLimitWindowSeconds = settings.SensitiveAdminRateLimitWindowSeconds,
            SensitiveAdminRateLimitQueueLimit = settings.SensitiveAdminRateLimitQueueLimit,
            SensitiveAdminRateLimitCooldownSeconds = settings.SensitiveAdminRateLimitCooldownSeconds,
            AllowedIpRanges = settings.GetAllowedIpRanges(),
            BlockedIpRanges = settings.GetBlockedIpRanges(),
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }
}