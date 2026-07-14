
namespace UnifiedUserSystem.src.Domain.Common;

public sealed record LocalizedMessage(string English, string Persian)
{
    public string Get(string? language)
    {
        return string.Equals(language, "fa", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(language, "fa_IR", StringComparison.OrdinalIgnoreCase)
               ? Persian
               : English;
    }
}
