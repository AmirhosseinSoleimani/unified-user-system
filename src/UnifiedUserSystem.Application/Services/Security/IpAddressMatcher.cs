
using System.Net;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Services.Security;

public static class IpAddressMatcher
{
    public static string NormalizeIpAddress(string ipAddress)
    {
        ipAddress = Guard.NotEmpty(ipAddress, nameof(ipAddress));

        if (!IPAddress.TryParse(ipAddress, out var parsed))
            throw new DomainException("IP address is invalid.");

        return parsed.ToString().ToLowerInvariant();
    }

    public static bool IsMatch(string ruleIpOrCidr, string ipAddress)
    {
        var normalizedIp = NormalizeIpAddress(ipAddress);

        if (!ruleIpOrCidr.Contains('/'))
            return string.Equals(NormalizeIpAddress(ruleIpOrCidr), normalizedIp, StringComparison.OrdinalIgnoreCase);

        var parts = ruleIpOrCidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            throw new DomainException("IP address or CIDR is invalid.");

        if (!IPAddress.TryParse(parts[0], out var network) || !IPAddress.TryParse(normalizedIp, out var ip))
            throw new DomainException("IP address or CIDR is invalid.");

        if (network.AddressFamily != ip.AddressFamily)
            return false;

        if (!int.TryParse(parts[1], out var prefixLength))
            throw new DomainException("IP address or CIDR is invalid.");

        var networkBytes = network.GetAddressBytes();
        var ipBytes = ip.GetAddressBytes();
        var maxPrefix = networkBytes.Length * 8;

        if (prefixLength < 0 || prefixLength > maxPrefix)
            throw new DomainException("IP address or CIDR is invalid.");

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != ipBytes[i])
                return false;
        }

        if (remainingBits == 0)
            return true;

        var mask = (byte)(0xFF << (8 - remainingBits));
        return (networkBytes[fullBytes] & mask) == (ipBytes[fullBytes] & mask);
    }
}
