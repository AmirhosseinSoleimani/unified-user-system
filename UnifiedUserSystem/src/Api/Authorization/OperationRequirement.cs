using Microsoft.AspNetCore.Authorization;

namespace UnifiedUserSystem.src.Api.Authorization
{
    public sealed class OperationRequirement : IAuthorizationRequirement
    {
        public OperationRequirement(string operationKey)
        {
            operationKey = (operationKey ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(operationKey))
                throw new ArgumentException("OperationKey is required.", nameof(operationKey));

            OperationKey = operationKey;
        }

        public string OperationKey { get; }
    }
}