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
            throw new DomainException($"Password must be at last {MinLength} characters");

        if (password.Length > MaxLength)
            throw new DomainException($"Password must be at most {MaxLength} characters");

        if (RequireUpper && !password.Any(char.IsUpper))
            throw new DomainException("Password must contain at least one uppercase letter");

        if (RequireLower && !password.Any(char.IsLower))
            throw new DomainException("Password must contain at least one lowercase letter");

        if (RequireDigit && !password.Any(char.IsDigit))
            throw new DomainException("Password must contain at least one digit");

        if (RequireSpecial && !Regex.IsMatch(password, @"[!@#$%^&*()_\-+=\[\]{};:,.?/\\|~]"))
            throw new DomainException("Password must contain at least one special character");
    }
}
