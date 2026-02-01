using Microsoft.AspNetCore.Authorization;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public class OperationRequirement : IAuthorizationRequirement
    {
        public string OperationKey { get; }

        public OperationRequirement(string operationKey)
        {
            OperationKey = (operationKey ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(operationKey))
                throw new ArgumentException("OperationKey is required.", nameof(operationKey));
        }
    }
}
