
namespace UnifiedUserSystem.src.Application.Options;

public sealed class MfaOptions
{
    public int OtpLength { get; set; } = 6;
    public MfaEmailOptions Email { get; set; } = new();
    public MfaSmsOptions Sms { get; set; } = new();
}

public sealed class MfaEmailOptions
{
    public string From { get; set; } = "no-reply@example.com";
    public string Provider { get; set; } = "Development";
}

public sealed class MfaSmsOptions
{
    public string Provider { get; set; } = "Development";
    public string ApiKey { get; set; } = string.Empty;
}
