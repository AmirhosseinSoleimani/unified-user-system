using System.Globalization;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Localization;

public sealed class DictionaryBusinessMessageLocalizer : IBusinessMessageLocalizer
{
    public static DictionaryBusinessMessageLocalizer Instance { get; } = new();

    public string Get(
        string code,
        string locale,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string? fallbackMessage = null)
    {
        var template = string.Equals(code, DomainErrorCodes.DomainError, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(locale, LocalizationOptions.EnglishLocale, StringComparison.OrdinalIgnoreCase) &&
                       !string.IsNullOrWhiteSpace(fallbackMessage)
            ? fallbackMessage
            : FindTemplate(code, locale)
              ?? FindTemplate(code, LocalizationOptions.EnglishLocale)
              ?? fallbackMessage
              ?? code;

        return ReplaceParameters(template, parameters, locale);
    }

    private static string? FindTemplate(string code, string locale)
        => Catalogs.TryGetValue(locale, out var catalog) && catalog.TryGetValue(code, out var value)
            ? value
            : null;

    private static string ReplaceParameters(
        string template,
        IReadOnlyDictionary<string, object?>? parameters,
        string locale)
    {
        if (parameters is null || parameters.Count == 0)
            return template;

        foreach (var (key, value) in parameters)
        {
            template = template.Replace(
                "{" + key + "}",
                ConvertParameterValue(key, value, locale),
                StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private static string ConvertParameterValue(
        string key,
        object? value,
        string locale)
    {
        if (string.Equals(key, "field", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(locale, LocalizationOptions.PersianLocale, StringComparison.OrdinalIgnoreCase) &&
            value is string field)
        {
            return field switch
            {
                "Email" => "ایمیل",
                "Username" => "نام کاربری",
                "FirstName" => "نام",
                "LastName" => "نام خانوادگی",
                "PhoneNumber" => "شماره تلفن",
                "Password" => "گذرواژه",
                "EmailOrUsername" => "ایمیل یا نام کاربری",
                "ChallengeId" => "شناسه چالش",
                _ => field
            };
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static IReadOnlyDictionary<string, string> EnglishMessages { get; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DomainErrorCodes.DomainError] = "A business rule was violated.",
            [DomainErrorCodes.ValidationNull] = "{field} cannot be null.",
            [DomainErrorCodes.ValidationRequired] = "{field} is required.",
            [DomainErrorCodes.ValidationMaxLength] = "{field} cannot be longer than {max} characters.",
            [DomainErrorCodes.ValidationMinLength] = "{field} must contain at least {min} characters.",
            [DomainErrorCodes.ValidationLengthRange] = "{field} must contain between {min} and {max} characters.",
            [DomainErrorCodes.ValidationFailed] = "The supplied value for '{field}' is invalid.",
            [DomainErrorCodes.RequestRequired] = "The request body is required.",
            [DomainErrorCodes.PasswordMinLength] = "The password must contain at least {min} characters.",
            [DomainErrorCodes.PasswordMaxLength] = "The password cannot contain more than {max} characters.",
            [DomainErrorCodes.PasswordUppercaseRequired] = "The password must contain at least one uppercase letter.",
            [DomainErrorCodes.PasswordLowercaseRequired] = "The password must contain at least one lowercase letter.",
            [DomainErrorCodes.PasswordDigitRequired] = "The password must contain at least one digit.",
            [DomainErrorCodes.PasswordSpecialRequired] = "The password must contain at least one special character.",
            [DomainErrorCodes.UsernameReserved] = "This username is reserved.",
            [DomainErrorCodes.UsernameInvalid] = "The username format is invalid.",
            [DomainErrorCodes.EmailInvalid] = "The email address format is invalid.",
            [DomainErrorCodes.PhoneNumberInvalid] = "The phone number format is invalid.",
            [DomainErrorCodes.RoleIdInvalid] = "The role identifier is invalid.",
            [DomainErrorCodes.UnsupportedLocale] = "The locale '{locale}' is not supported. Supported locales: {supportedLocales}.",
            [DomainErrorCodes.LocaleFormatInvalid] = "The locale format is invalid.",
            [DomainErrorCodes.UserNotFound] = "The user was not found.",
            [DomainErrorCodes.EmailAlreadyExists] = "An account with this email address already exists.",
            [DomainErrorCodes.UsernameAlreadyExists] = "This username is already in use.",
            [DomainErrorCodes.DefaultRoleNotFound] = "The default user role is not configured.",
            [DomainErrorCodes.MfaNoChannelEnabled] = "Multi-factor authentication is enabled, but no verification channel is available.",
            [DomainErrorCodes.MfaChannelDisabled] = "The selected multi-factor authentication channel is disabled.",
            [DomainErrorCodes.MfaChannelInvalid] = "The selected multi-factor authentication channel is invalid.",
            [MessageCodes.AuthenticationFailed] = "Authentication failed.",
            [MessageCodes.MfaVerificationFailed] = "Multi-factor authentication verification failed.",
            [MessageCodes.RefreshTokenInvalid] = "The refresh token is invalid.",
            [MessageCodes.Unauthorized] = "You are not authorized to perform this operation.",
            [MessageCodes.Forbidden] = "Access to this resource is forbidden.",
            [MessageCodes.NotFound] = "The requested resource was not found.",
            [MessageCodes.Conflict] = "The request conflicts with the current state of the resource.",
            [MessageCodes.BadRequest] = "The request is invalid.",
            [MessageCodes.UnexpectedError] = "An unexpected error occurred.",
            [MessageCodes.RateLimitExceeded] = "Too many requests were sent. Please try again later.",
            [MessageCodes.IpAddressBlocked] = "Requests from this IP address are not allowed.",
            [MessageCodes.DomainErrorTitle] = "Error while performing the operation",
            [MessageCodes.LogoutSucceeded] = "You have been logged out successfully.",
            [MessageCodes.SessionsRevoked] = "All active sessions were revoked successfully.",
            [MessageCodes.PreferredLocaleUpdated] = "Your language preference was updated successfully."
};

    private static IReadOnlyDictionary<string, string> PersianMessages { get; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DomainErrorCodes.DomainError] = "یکی از قوانین کسب‌وکار رعایت نشده است",
            [DomainErrorCodes.ValidationNull] = "مقدار {field} نمی‌تواند تهی باشد",
            [DomainErrorCodes.ValidationRequired] = "وارد کردن {field} الزامی است",
            [DomainErrorCodes.ValidationMaxLength] = "طول {field} نمی‌تواند بیشتر از {max} کاراکتر باشد",
            [DomainErrorCodes.ValidationMinLength] = "طول {field} باید حداقل {min} کاراکتر باشد",
            [DomainErrorCodes.ValidationLengthRange] = "طول {field} باید بین {min} تا {max} کاراکتر باشد",
            [DomainErrorCodes.ValidationFailed] = "مقدار واردشده برای '{field}' معتبر نیست",
            [DomainErrorCodes.RequestRequired] = "ارسال بدنه درخواست الزامی است",
            [DomainErrorCodes.PasswordMinLength] = "گذرواژه باید حداقل {min} کاراکتر داشته باشد",
            [DomainErrorCodes.PasswordMaxLength] = "گذرواژه نمی‌تواند بیشتر از {max} کاراکتر داشته باشد",
            [DomainErrorCodes.PasswordUppercaseRequired] = "گذرواژه باید حداقل یک حرف بزرگ انگلیسی داشته باشد",
            [DomainErrorCodes.PasswordLowercaseRequired] = "گذرواژه باید حداقل یک حرف کوچک انگلیسی داشته باشد",
            [DomainErrorCodes.PasswordDigitRequired] = "گذرواژه باید حداقل یک عدد داشته باشد",
            [DomainErrorCodes.PasswordSpecialRequired] = "گذرواژه باید حداقل یک نویسه ویژه داشته باشد",
            [DomainErrorCodes.UsernameReserved] = "این نام کاربری رزرو شده است",
            [DomainErrorCodes.UsernameInvalid] = "ساختار نام کاربری معتبر نیست",
            [DomainErrorCodes.EmailInvalid] = "ساختار نشانی ایمیل معتبر نیست",
            [DomainErrorCodes.PhoneNumberInvalid] = "ساختار شماره تلفن معتبر نیست",
            [DomainErrorCodes.RoleIdInvalid] = "شناسه نقش معتبر نیست",
            [DomainErrorCodes.UnsupportedLocale] = "زبان '{locale}' پشتیبانی نمی‌شود. زبان‌های مجاز: {supportedLocales}",
            [DomainErrorCodes.LocaleFormatInvalid] = "ساختار کد زبان معتبر نیست",
            [DomainErrorCodes.UserNotFound] = "کاربر موردنظر یافت نشد",
            [DomainErrorCodes.EmailAlreadyExists] = "قبلاً حسابی با این نشانی ایمیل ایجاد شده است",
            [DomainErrorCodes.UsernameAlreadyExists] = "این نام کاربری قبلاً استفاده شده است",
            [DomainErrorCodes.DefaultRoleNotFound] = "نقش پیش‌فرض کاربر در سامانه پیکربندی نشده است",
            [DomainErrorCodes.MfaNoChannelEnabled] = "احراز هویت چندمرحله‌ای فعال است، اما هیچ کانال تأییدی در دسترس نیست",
            [DomainErrorCodes.MfaChannelDisabled] = "کانال انتخاب‌شده برای احراز هویت چندمرحله‌ای غیرفعال است",
            [DomainErrorCodes.MfaChannelInvalid] = "کانال انتخاب‌شده برای احراز هویت چندمرحله‌ای معتبر نیست",
            [MessageCodes.AuthenticationFailed] = "اطلاعات ورود صحیح نیست",
            [MessageCodes.MfaVerificationFailed] = "تأیید احراز هویت چندمرحله‌ای ناموفق بود",
            [MessageCodes.RefreshTokenInvalid] = "توکن تمدید معتبر نیست",
            [MessageCodes.Unauthorized] = "شما مجوز انجام این عملیات را ندارید",
            [MessageCodes.Forbidden] = "دسترسی به این منبع مجاز نیست",
            [MessageCodes.NotFound] = "منبع درخواستی یافت نشد",
            [MessageCodes.Conflict] = "درخواست با وضعیت فعلی منبع تداخل دارد",
            [MessageCodes.BadRequest] = "درخواست ارسال‌شده معتبر نیست",
            [MessageCodes.UnexpectedError] = "خطای پیش‌بینی‌نشده‌ای رخ داده است",
            [MessageCodes.RateLimitExceeded] = "تعداد درخواست‌ها بیش از حد مجاز است. کمی بعد دوباره تلاش کنید",
            [MessageCodes.IpAddressBlocked] = "دسترسی از این نشانی IP مجاز نیست",
            [MessageCodes.DomainErrorTitle] = "خطا در انجام عملیات",
            [MessageCodes.LogoutSucceeded] = "خروج از حساب با موفقیت انجام شد",
            [MessageCodes.SessionsRevoked] = "تمام نشست‌های فعال با موفقیت لغو شدند",
            [MessageCodes.PreferredLocaleUpdated] = "زبان موردنظر شما با موفقیت ذخیره شد"
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Catalogs =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [LocalizationOptions.EnglishLocale] = EnglishMessages,
            [LocalizationOptions.PersianLocale] = PersianMessages
        };

}
