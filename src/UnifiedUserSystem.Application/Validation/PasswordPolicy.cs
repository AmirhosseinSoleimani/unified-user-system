using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Validation;

public sealed class PasswordPolicy : IPasswordPolicy
{
    public int MinLength { get; } = 8;
    public int MaxLength { get; } = 64;
    public bool RequireUpper { get; } = true;
    public bool RequireLower { get; } = true;
    public bool RequireDigit { get; } = true;
    public bool RequireSpecial { get; } = true;

    public void Validate(string password)
    {
        password = (password ?? string.Empty).Trim();

        if (password.Length < MinLength)
            throw DomainException.For(
                DomainErrorCodes.PasswordMinLength,
                Parameters(("min", MinLength)));

        if (password.Length > MaxLength)
            throw DomainException.For(
                DomainErrorCodes.PasswordMaxLength,
                Parameters(("max", MaxLength)));

        if (RequireUpper && !password.Any(char.IsUpper))
            throw DomainException.For(DomainErrorCodes.PasswordUppercaseRequired);

        if (RequireLower && !password.Any(char.IsLower))
            throw DomainException.For(DomainErrorCodes.PasswordLowercaseRequired);

        if (RequireDigit && !password.Any(char.IsDigit))
            throw DomainException.For(DomainErrorCodes.PasswordDigitRequired);

        if (RequireSpecial && !Regex.IsMatch(password, @"[!@#$%^&*()_\-+=\[\]{};:,.?/\\|~]"))
            throw DomainException.For(DomainErrorCodes.PasswordSpecialRequired);
    }

    private static IReadOnlyDictionary<string, object?> Parameters(
        params (string Key, object? Value)[] values)
        => values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
}
