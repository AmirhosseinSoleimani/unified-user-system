using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public sealed class OperationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public OperationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (OperationPolicyNames.TryGetOperationKey(policyName, out var operationKey))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new OperationRequirement(operationKey))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}