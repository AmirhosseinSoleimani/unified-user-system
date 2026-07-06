namespace UnifiedUserSystem.src.Api.Middlewares
{
    public static class SecurityRateLimitPolicies
    {
        public const string Auth = "AuthRateLimit";
        public const string SensitiveAdmin = "SensitiveAdminRateLimit";
    }
}