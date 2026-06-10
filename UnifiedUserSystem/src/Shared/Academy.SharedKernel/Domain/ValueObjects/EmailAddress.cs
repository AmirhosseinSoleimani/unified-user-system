using Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;
using Academy.src.Shared.Academy.SharedKernel.Domain.Primitives;
using Academy.src.Shared.Academy.SharedKernel.Domain.Utilities;
using System.Text.RegularExpressions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.ValueObjects;

public sealed partial class EmailAddress : ValueObject
{
    private const int MaxLength = 320;

    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string value)
    {
        var normalized = TextNormalizer.NormalizeEmail(value);

        if (normalized.Length > MaxLength)
            throw new DomainValidationException("Email address is too long.");

        if (!EmailRegex().IsMatch(normalized))
            throw new DomainValidationException("Email address format is invalid.");

        return new EmailAddress(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
