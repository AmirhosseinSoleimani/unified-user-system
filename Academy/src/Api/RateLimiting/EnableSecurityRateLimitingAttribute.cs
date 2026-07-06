namespace UnifiedUserSystem.src.Api.RateLimiting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EnableSecurityRateLimitingAttribute : Attribute
    {
        public EnableSecurityRateLimitingAttribute(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; }
    }
}