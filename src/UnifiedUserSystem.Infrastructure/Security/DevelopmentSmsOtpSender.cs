using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class DevelopmentSmsOtpSender : ISmsOtpSender
{
    private readonly ILogger<DevelopmentSmsOtpSender> _logger;
    private readonly MfaOptions _options;

    public DevelopmentSmsOtpSender(
        ILogger<DevelopmentSmsOtpSender> logger,
        IOptions<MfaOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(User user, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Development SMS OTP sender. Provider={Provider}, To={PhoneNumber}, ExpiresAt={ExpiresAt}, Otp={OtpCode}",
            _options.Sms.Provider,
            user.PhoneNumber,
            expiresAt,
            otpCode);

        return Task.CompletedTask;
    }
}
