using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Api.Options;

namespace UnifiedUserSystem.UnitTests.Application.Options;

public sealed class ForwardedHeadersOptionsSetupTests
{
    [Fact]
    public void Configure_Should_EnableForwardedForAndForwardedProto()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>());
        var setup = new ForwardedHeadersOptionsSetup(configuration);
        var options = new ForwardedHeadersOptions();

        setup.Configure(options);

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
        Assert.Equal(1, options.ForwardLimit);
    }

    [Fact]
    public void Configure_Should_LoadKnownProxiesFromConfiguration()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:KnownProxies:0"] = "10.0.0.10",
            ["ForwardedHeaders:KnownProxies:1"] = "192.168.1.10"
        });

        var setup = new ForwardedHeadersOptionsSetup(configuration);
        var options = new ForwardedHeadersOptions();

        setup.Configure(options);

        Assert.Contains(IPAddress.Parse("10.0.0.10"), options.KnownProxies);
        Assert.Contains(IPAddress.Parse("192.168.1.10"), options.KnownProxies);
    }

    [Fact]
    public void Configure_Should_LoadKnownNetworksFromConfiguration()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:KnownNetworks:0"] = "10.0.0.0/24",
            ["ForwardedHeaders:KnownNetworks:1"] = "192.168.0.0/16"
        });

        var setup = new ForwardedHeadersOptionsSetup(configuration);
        var options = new ForwardedHeadersOptions();

        setup.Configure(options);

        Assert.Contains(options.KnownNetworks, network =>
            network.Prefix.Equals(IPAddress.Parse("10.0.0.0")) &&
            network.PrefixLength == 24);

        Assert.Contains(options.KnownNetworks, network =>
            network.Prefix.Equals(IPAddress.Parse("192.168.0.0")) &&
            network.PrefixLength == 16);
    }

    [Fact]
    public void Configure_Should_IgnoreInvalidProxyAndNetworkValues()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:KnownProxies:0"] = "not-an-ip",
            ["ForwardedHeaders:KnownNetworks:0"] = "not-a-network",
            ["ForwardedHeaders:KnownNetworks:1"] = "10.0.0.0/not-number"
        });

        var setup = new ForwardedHeadersOptionsSetup(configuration);
        var options = new ForwardedHeadersOptions();

        setup.Configure(options);

        Assert.Empty(options.KnownProxies);
        Assert.Empty(options.KnownNetworks);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
