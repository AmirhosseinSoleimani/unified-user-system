using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace UnifiedUserSystem.src.Api.Options;

public sealed class ForwardedHeadersOptionsSetup : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly IConfiguration _configuration;

    public ForwardedHeadersOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();

        var section = _configuration.GetSection("ForwardedHeaders");

        foreach (var proxy in section.GetSection("KnownProxies").Get<string[]>() ?? Array.Empty<string>())
        {
            if (IPAddress.TryParse(proxy, out var ipAddress))
                options.KnownProxies.Add(ipAddress);
        }

        foreach (var network in section.GetSection("KnownNetworks").Get<string[]>() ?? Array.Empty<string>())
        {
            var parts = network.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                continue;

            if (IPAddress.TryParse(parts[0], out var prefix) &&
                int.TryParse(parts[1], out var prefixLength))
            {
                options.KnownNetworks.Add(
                    new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength));
            }
        }
    }
}
