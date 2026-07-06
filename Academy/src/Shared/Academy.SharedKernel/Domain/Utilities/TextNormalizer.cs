using System.Text.RegularExpressions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Utilities;

public static partial class TextNormalizer
{
    public static string NormalizeRequired(string value)
    {
        return NormalizeWhitespace(value).Trim();
    }

    public static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return NormalizeWhitespace(value).Trim();
    }

    public static string NormalizeEmail(string value)
    {
        return NormalizeRequired(value).ToLowerInvariant();
    }

    public static string NormalizeUsername(string value)
    {
        return NormalizeRequired(value).ToLowerInvariant();
    }

    public static string NormalizeWhitespace(string value)
    {
        return WhitespaceRegex().Replace(value, " ");
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}