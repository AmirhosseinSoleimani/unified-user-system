namespace UnifiedUserSystem.src.Api.Localization;

public interface IBusinessMessageLocalizer
{
    string Get(
        string code,
        string locale,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string? fallbackMessage = null);
}

