namespace UnifiedUserSystem.src.Contracts.DTOs.Localization;

public sealed record ErrorMessageResponse(
    Guid Id,
    string Key,
    string EnglishText,
    string PersianText,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

