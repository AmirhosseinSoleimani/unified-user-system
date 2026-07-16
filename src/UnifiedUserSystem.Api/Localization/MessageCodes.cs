namespace UnifiedUserSystem.src.Api.Localization;

public static class MessageCodes
{
    public const string AuthenticationFailed = "auth.authentication_failed";
    public const string MfaVerificationFailed = "auth.mfa_verification_failed";
    public const string RefreshTokenInvalid = "auth.refresh_token_invalid";
    public const string Unauthorized = "http.unauthorized";
    public const string Forbidden = "http.forbidden";
    public const string NotFound = "http.not_found";
    public const string Conflict = "http.conflict";
    public const string BadRequest = "http.bad_request";
    public const string UnexpectedError = "http.unexpected_error";
    public const string RateLimitExceeded = "http.rate_limit_exceeded";
    public const string IpAddressBlocked = "http.ip_address_blocked";
    public const string DomainErrorTitle = "http.domain_error";

    public const string LogoutSucceeded = "auth.logout_succeeded";
    public const string SessionsRevoked = "auth.sessions_revoked";
    public const string PreferredLocaleUpdated = "profile.preferred_locale_updated";
}
