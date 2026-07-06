using Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;
using Academy.src.Shared.Academy.SharedKernel.Domain.Primitives;
using System.Text.RegularExpressions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.ValueObjects;

public sealed partial class Slug : ValueObject
{
    private const int MaxLength = 160;

    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Slug is required.");

        var normalized = value.Trim().ToLowerInvariant();

        normalized = WhitespaceRegex().Replace(normalized, "-");
        normalized = InvalidCharactersRegex().Replace(normalized, string.Empty);
        normalized = RepeatedDashRegex().Replace(normalized, "-");
        normalized = normalized.Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainValidationException("Slug is invalid.");

        if (normalized.Length > MaxLength)
            throw new DomainValidationException("Slug is too long.");

        return new Slug(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharactersRegex();

    [GeneratedRegex(@"\-+", RegexOptions.Compiled)]
    private static partial Regex RepeatedDashRegex();
}
