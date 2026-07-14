namespace UnifiedUserSystem.src.Contracts.DTOs.Localization;

public sealed class UpsertErrorMessageRequest
{
    public string Key { get; set; } = default!;
    public string EnglishText { get; set; } = default!;
    public string PersianText { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
