namespace UnifiedUserSystem.src.Api.Localization;

public interface IRequestLocaleResolver
{
    string Resolve(HttpContext context);

    string? ResolveHeader(string? acceptLanguageHeader);
}

