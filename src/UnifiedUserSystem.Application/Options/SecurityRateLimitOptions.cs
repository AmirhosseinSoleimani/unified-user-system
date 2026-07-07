namespace UnifiedUserSystem.src.Application.Options;

public sealed class SecurityRateLimitOptions
{
    public SecurityRateLimitPolicyOptions Auth { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60
    };

    public SecurityRateLimitPolicyOptions SensitiveAdmin { get; set; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60
    };
}

public sealed class SecurityRateLimitPolicyOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
}