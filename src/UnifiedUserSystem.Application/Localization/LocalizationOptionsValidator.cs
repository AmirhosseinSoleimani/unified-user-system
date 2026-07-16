
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.src.Application.Localization;

public sealed class LocalizationOptionsValidator : IValidateOptions<LocalizationOptions>
{
    public ValidateOptionsResult Validate(string? name, LocalizationOptions options)
       => options.IsValid()
           ? ValidateOptionsResult.Success
           : ValidateOptionsResult.Fail(
               "Localization configuration is invalid. SupportedLocales must contain fa-IR and en-US, DefaultLocale must be supported, and GeoIp settings must be valid when enabled.");
}