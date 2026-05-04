using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public sealed class OperationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public const string prefix = "OP:";

        public OperationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!string.IsNullOrWhiteSpace(policyName) && policyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var opKey = policyName.Substring(prefix.Length).Trim().ToLowerInvariant();

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new OperationRequirement(opKey))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
