using Microsoft.Extensions.Logging;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class DevelopmentEmailOtpSender : IEmailOtpSender
{
    private readonly ILogger<DevelopmentEmailOtpSender> _logger;
    private readonly MfaOptions _options;

    public DevelopmentEmailOtpSender(
        ILogger<DevelopmentEmailOtpSender> logger,
        IOptions<MfaOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(User user, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Development email OTP sender. Provider={Provider}, From={From}, To={Email}, ExpiresAt={ExpiresAt}, Otp={OtpCode}",
            _options.Email.Provider,
            _options.Email.From,
            user.Email,
            expiresAt,
            otpCode);

        return Task.CompletedTask;
    }
}
