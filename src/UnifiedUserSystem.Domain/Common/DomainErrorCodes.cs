namespace UnifiedUserSystem.src.Domain.Common;

public static class DomainErrorCodes
{
    public const string DomainError = "domain.error";

    public const string ValidationNull = "validation.null";
    public const string ValidationRequired = "validation.required";
    public const string ValidationMaxLength = "validation.max_length";
    public const string ValidationMinLength = "validation.min_length";
    public const string ValidationLengthRange = "validation.length_range";
    public const string ValidationFailed = "validation.failed";
    public const string RequestRequired = "request.required";
    public const string PasswordMinLength = "security.password_min_length";
    public const string PasswordMaxLength = "security.password_max_length";
    public const string PasswordUppercaseRequired = "security.password_uppercase_required";
    public const string PasswordLowercaseRequired = "security.password_lowercase_required";
    public const string PasswordDigitRequired = "security.password_digit_required";
    public const string PasswordSpecialRequired = "security.password_special_required";

    public const string UsernameReserved = "identity.username_reserved";
    public const string UsernameInvalid = "identity.username_format_invalid";
    public const string EmailInvalid = "identity.email_format_invalid";
    public const string PhoneNumberInvalid = "identity.phone_number_format_invalid";
    public const string RoleIdInvalid = "identity.role_id_invalid";
    public const string UnsupportedLocale = "identity.unsupported_locale";
    public const string LocaleFormatInvalid = "identity.locale_format_invalid";
    public const string UserNotFound = "identity.user_not_found";
    public const string EmailAlreadyExists = "identity.email_already_exists";
    public const string UsernameAlreadyExists = "identity.username_already_exists";
    public const string PhoneNumberAlreadyExists = "identity.phone_number_already_exists";
    public const string DefaultRoleNotFound = "identity.default_role_not_found";

    public const string MfaNoChannelEnabled = "security.mfa_no_channel_enabled";
    public const string MfaChannelDisabled = "security.mfa_channel_disabled";
    public const string MfaChannelInvalid = "security.mfa_channel_invalid";
}
