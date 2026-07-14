using UnifiedUserSystem.src.Domain.Common;


namespace UnifiedUserSystem.src.Domain.Localization.Entities;

public sealed class ErrorMessage : AuditableEntity<Guid>
{
    public const int KeyMaxLength = 128;
    public const int TextMaxLength = 1024;

    public string Key { get; private set; } = default!;
    public string EnglishText { get; private set; } = default!;
    public string PersianText { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private ErrorMessage()
    {
    }

    public static ErrorMessage Create(
        string key,
        string englishText,
        string persianText,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        var message = new ErrorMessage
        {
            Id = Guid.NewGuid(),
            IsActive = true
        };

        message.Update(key, englishText, persianText, isActive: true, nowUtc, actorUserId);
        message.SetCreated(nowUtc, actorUserId);

        return message;
    }

    public void Update(
        string key,
        string englishText,
        string persianText,
        bool isActive,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        Key = NormalizeKey(key);
        EnglishText = NormalizeRequired(englishText, nameof(EnglishText), TextMaxLength);
        PersianText = NormalizeRequired(persianText, nameof(PersianText), TextMaxLength);
        IsActive = isActive;

        Touch(nowUtc, actorUserId);
    }

    public LocalizedMessage ToLocalizedMessage()
        => new(EnglishText, PersianText);

    public static string NormalizeKey(string key)
    {
        key = NormalizeRequired(key, nameof(Key), KeyMaxLength).ToLowerInvariant();

        foreach (var ch in key)
        {
            var isAllowed =
                ch >= 'a' && ch <= 'z' ||
                ch >= '0' && ch <= '9' ||
                ch == '.' || ch == '-' || ch == '_';

            if (!isAllowed)
                throw new DomainException("Error message key contains invalid characters.");
        }

        return key;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        value = (value ?? string.Empty).Trim();
        Guard.NotEmpty(value, fieldName);
        Guard.MaxLen(value, maxLength, fieldName);
        return value;
    }
}

