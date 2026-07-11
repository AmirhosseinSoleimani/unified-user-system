using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Application.Security;

[Trait("Category", "Security")]
public sealed class IpAddressMatcherTests
{
    [Theory]
    [InlineData("192.168.1.10", "192.168.1.10")]
    [InlineData("127.0.0.1", "127.0.0.1")]
    [InlineData("2001:db8::1", "2001:db8::1")]
    public void Direct_ip_match_returns_true(string rule, string candidate)
    {
        IpAddressMatcher.IsMatch(rule, candidate).Should().BeTrue();
    }

    [Theory]
    [InlineData("192.168.1.10", "192.168.1.11")]
    [InlineData("10.0.0.1", "10.0.0.2")]
    [InlineData("2001:db8::1", "2001:db8::2")]
    public void Direct_ip_non_match_returns_false(string rule, string candidate)
    {
        IpAddressMatcher.IsMatch(rule, candidate).Should().BeFalse();
    }

    [Theory]
    [InlineData("10.0.0.0/8", "10.20.30.40")]
    [InlineData("192.168.1.0/24", "192.168.1.200")]
    [InlineData("172.16.0.0/12", "172.31.255.255")]
    [InlineData("2001:db8::/32", "2001:db8:abcd::1")]
    public void Cidr_match_returns_true(string rule, string candidate)
    {
        IpAddressMatcher.IsMatch(rule, candidate).Should().BeTrue();
    }

    [Theory]
    [InlineData("10.0.0.0/8", "11.0.0.1")]
    [InlineData("192.168.1.0/24", "192.168.2.1")]
    [InlineData("172.16.0.0/12", "172.32.0.1")]
    [InlineData("2001:db8::/32", "2001:db9::1")]
    public void Cidr_non_match_returns_false(string rule, string candidate)
    {
        IpAddressMatcher.IsMatch(rule, candidate).Should().BeFalse();
    }

    [Fact]
    public void Address_family_mismatch_returns_false()
    {
        IpAddressMatcher
            .IsMatch("10.0.0.0/8", "2001:db8::1")
            .Should()
            .BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-ip")]
    public void Normalize_rejects_invalid_ip(string value)
    {
        var action = () =>
            IpAddressMatcher.NormalizeIpAddress(value);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Normalize_returns_canonical_ipv6_representation()
    {
        var normalized =
            IpAddressMatcher.NormalizeIpAddress("2001:0db8:0:0:0:0:0:1");

        normalized.Should().Be("2001:db8::1");
    }

    [Theory]
    [InlineData("10.0.0.0/not-a-prefix", "10.0.0.1")]
    [InlineData("10.0.0.0/33", "10.0.0.1")]
    [InlineData("invalid/8", "10.0.0.1")]
    public void Invalid_cidr_is_rejected(string rule, string candidate)
    {
        var action = () =>
            IpAddressMatcher.IsMatch(rule, candidate);

        action.Should().Throw<DomainException>();
    }
}
