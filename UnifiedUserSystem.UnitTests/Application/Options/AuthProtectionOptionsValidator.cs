using Microsoft.Extensions.Options;

namespace UnifiedUserSystem.src.Application.Options
{
    public sealed class AuthProtectionOptionsValidator : IValidateOptions<AuthProtectionOptions>
    {
        public ValidateOptionsResult Validate(string? name, AuthProtectionOptions options)
        {
            var failures = new List<string>();

            if (options.MaxFailedAttemptsPerIdentity <= 0)
                failures.Add($"{nameof(options.MaxFailedAttemptsPerIdentity)} must be greater than 0.");

            if (options.MaxFailedAttemptsPerClient <= 0)
                failures.Add($"{nameof(options.MaxFailedAttemptsPerClient)} must be greater than 0.");

            if (options.FailedAttemptWindowMinutes <= 0)
                failures.Add($"{nameof(options.FailedAttemptWindowMinutes)} must be greater than 0.");

            if (options.LockoutMinutes <= 0)
                failures.Add($"{nameof(options.LockoutMinutes)} must be greater than 0.");

            if (options.CooldownSeconds < 0)
                failures.Add($"{nameof(options.CooldownSeconds)} must be greater than or equal to 0.");

            if (options.AuthRateLimitPermitLimit <= 0)
                failures.Add($"{nameof(options.AuthRateLimitPermitLimit)} must be greater than 0.");

            if (options.AuthRateLimitWindowSeconds <= 0)
                failures.Add($"{nameof(options.AuthRateLimitWindowSeconds)} must be greater than 0.");

            if (options.AuthRateLimitQueueLimit < 0)
                failures.Add($"{nameof(options.AuthRateLimitQueueLimit)} must be greater than or equal to 0.");

            if (options.SensitiveAdminRateLimitPermitLimit <= 0)
                failures.Add($"{nameof(options.SensitiveAdminRateLimitPermitLimit)} must be greater than 0.");

            if (options.SensitiveAdminRateLimitWindowSeconds <= 0)
                failures.Add($"{nameof(options.SensitiveAdminRateLimitWindowSeconds)} must be greater than 0.");

            if (options.SensitiveAdminRateLimitQueueLimit < 0)
                failures.Add($"{nameof(options.SensitiveAdminRateLimitQueueLimit)} must be greater than or equal to 0.");

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }
}