
using System.Security.Cryptography;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class OtpGenerator : IOtpGenerator
{
    public string Generate(int length)
    {
        if (length < 4 || length > 10)
            throw new ArgumentOutOfRangeException(nameof(length), "OTP length must be between 4 and 10.");

        var chars = new char[length];

        for (var i = 0; i < chars.Length; i++)
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));

        return new string(chars);
    }
}
