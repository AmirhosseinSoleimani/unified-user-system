
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Domain.Security.IpRules;

[Trait("Category", "Domain")]
[Trait("Entity", "IpRule")]
public sealed class IpRuleCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveRule()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Id.Should().NotBeEmpty();

        rule.IpAddressOrCidr.Should().Be(IpRuleTestFactory.ValidIpv4);
        rule.NormalizedIpAddressOrCidr.Should()
            .Be(IpRuleTestFactory.ValidIpv4);

        rule.RuleType.Should().Be(IpRuleType.Block);
        rule.Reason.Should().Be(IpRuleTestFactory.ValidReason);

        rule.IsActive.Should().BeTrue();
        rule.DisabledAt.Should().BeNull();

        rule.IsDeleted.Should().BeFalse();
        rule.DeletedAt.Should().BeNull();
        rule.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithActor_ShouldInitializeAuditFields()
    {
        var rule = IpRuleTestFactory.Create(
            actorUserId: IpRuleTestFactory.ActorUserId);

        rule.CreatedAt.Should().Be(IpRuleTestFactory.CreatedAt);
        rule.UpdatedAt.Should().Be(IpRuleTestFactory.CreatedAt);

        rule.CreatedByUserId.Should()
            .Be(IpRuleTestFactory.ActorUserId);

        rule.UpdatedByUserId.Should()
            .Be(IpRuleTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldKeepAuditActorNull()
    {
        var rule = IpRuleTestFactory.Create(actorUserId: null);

        rule.CreatedByUserId.Should().BeNull();
        rule.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldGenerateDifferentIds()
    {
        var firstRule = IpRuleTestFactory.Create();

        var secondRule = IpRuleTestFactory.Create(
            ipAddressOrCidr: "192.168.1.11");

        firstRule.Id.Should().NotBe(secondRule.Id);
    }

    [Theory]
    [InlineData(IpRuleType.Block)]
    [InlineData(IpRuleType.Allow)]
    [InlineData(IpRuleType.Restrict)]
    public void Create_WithDefinedRuleType_ShouldSetRuleType(
        IpRuleType ruleType)
    {
        var rule = IpRuleTestFactory.Create(ruleType: ruleType);

        rule.RuleType.Should().Be(ruleType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Create_WithUndefinedRuleType_ShouldThrow(
        int ruleTypeValue)
    {
        var action = () => IpRuleTestFactory.Create(
            ruleType: (IpRuleType)ruleTypeValue);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*IP rule type is invalid*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenIpAddressMissing_ShouldThrow(
        string? ipAddressOrCidr)
    {
        var action = () => IpRuleTestFactory.Create(
            ipAddressOrCidr: ipAddressOrCidr!);

        action.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("192.168.1.256")]
    [InlineData("192.168.1.-1")]
    [InlineData("example.com")]
    [InlineData("192.168.1.1/")]
    [InlineData("/24")]
    [InlineData("192.168.1.1/not-number")]
    [InlineData("192.168.1.1/24/5")]
    [InlineData("2001:db8::zz")]
    public void Create_WhenIpAddressOrCidrInvalid_ShouldThrow(
    string ipAddressOrCidr)
    {
        var action = () => IpRuleTestFactory.Create(
            ipAddressOrCidr: ipAddressOrCidr);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*IP address or CIDR is invalid*");
    }

    [Fact]
    public void Create_WithShortIpv4Notation_ShouldNormalizeAddress()
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: "192.168.1");

        rule.IpAddressOrCidr.Should().Be("192.168.0.1");
        rule.NormalizedIpAddressOrCidr.Should().Be("192.168.0.1");
    }

    [Theory]
    [InlineData("192.168.1.1/-1")]
    [InlineData("192.168.1.1/33")]
    [InlineData("10.0.0.1/100")]
    [InlineData("2001:db8::1/-1")]
    [InlineData("2001:db8::1/129")]
    public void Create_WhenCidrPrefixOutsideAllowedRange_ShouldThrow(
        string ipAddressOrCidr)
    {
        var action = () => IpRuleTestFactory.Create(
            ipAddressOrCidr: ipAddressOrCidr);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*IP address or CIDR is invalid*");
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("127.0.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("10.0.0.1")]
    public void Create_WithValidIpv4Address_ShouldCreateRule(
        string ipAddress)
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: ipAddress);

        rule.IpAddressOrCidr.Should().Be(ipAddress);
        rule.NormalizedIpAddressOrCidr.Should().Be(ipAddress);
    }

    [Theory]
    [InlineData("192.168.1.0/24")]
    [InlineData("10.0.0.0/8")]
    [InlineData("0.0.0.0/0")]
    [InlineData("255.255.255.255/32")]
    public void Create_WithValidIpv4Cidr_ShouldCreateRule(
        string cidr)
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: cidr);

        rule.IpAddressOrCidr.Should().Be(cidr);
        rule.NormalizedIpAddressOrCidr.Should().Be(cidr);
    }

    [Theory]
    [InlineData("2001:db8::1")]
    [InlineData("::1")]
    [InlineData("::")]
    [InlineData("fe80::1")]
    public void Create_WithValidIpv6Address_ShouldCreateRule(
        string ipAddress)
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: ipAddress);

        rule.IpAddressOrCidr.Should()
            .Be(rule.NormalizedIpAddressOrCidr);

        rule.IpAddressOrCidr.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("2001:db8::/32")]
    [InlineData("::/0")]
    [InlineData("::1/128")]
    [InlineData("fe80::/10")]
    public void Create_WithValidIpv6Cidr_ShouldCreateRule(
        string cidr)
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: cidr);

        rule.IpAddressOrCidr.Should()
            .Be(rule.NormalizedIpAddressOrCidr);

        rule.IpAddressOrCidr.Should().EndWith(
            cidr[cidr.LastIndexOf('/')..]);
    }

    [Theory]
    [InlineData("192.168.001.001", "192.168.1.1")]
    [InlineData("127.000.000.001", "127.0.0.1")]
    public void Create_WithNonCanonicalIpv4_ShouldNormalizeAddress(
        string input,
        string expected)
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: input);

        rule.IpAddressOrCidr.Should().Be(expected);
        rule.NormalizedIpAddressOrCidr.Should().Be(expected);
    }

    [Fact]
    public void Create_WithExpandedIpv6_ShouldStoreCanonicalAddress()
    {
        const string expanded =
            "2001:0db8:0000:0000:0000:0000:0000:0001";

        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: expanded);

        rule.IpAddressOrCidr.Should().Be("2001:db8::1");
        rule.NormalizedIpAddressOrCidr.Should().Be("2001:db8::1");
    }

    [Fact]
    public void Create_WithExpandedIpv6Cidr_ShouldStoreCanonicalCidr()
    {
        const string expanded =
            "2001:0db8:0000:0000:0000:0000:0000:0000/32";

        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: expanded);

        rule.IpAddressOrCidr.Should().Be("2001:db8::/32");
        rule.NormalizedIpAddressOrCidr.Should()
            .Be("2001:db8::/32");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WhenReasonMissing_ShouldStoreNull(
        string? reason)
    {
        var rule = IpRuleTestFactory.Create(reason: reason);

        rule.Reason.Should().BeNull();
    }

    [Fact]
    public void Create_WhenReasonHasSurroundingWhitespace_ShouldTrimReason()
    {
        var rule = IpRuleTestFactory.Create(
            reason: " Suspicious activity ");

        rule.Reason.Should().Be("Suspicious activity");
    }

    [Fact]
    public void Create_WhenReasonHasMaximumLength_ShouldCreateRule()
    {
        var reason = IpRuleTestFactory.CreateString(
            IpRule.ReasonMaxLength);

        var rule = IpRuleTestFactory.Create(reason: reason);

        rule.Reason.Should().HaveLength(IpRule.ReasonMaxLength);
    }

    [Fact]
    public void Create_WhenReasonExceedsMaximumLength_ShouldThrow()
    {
        var reason = IpRuleTestFactory.CreateString(
            IpRule.ReasonMaxLength + 1);

        var action = () => IpRuleTestFactory.Create(reason: reason);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenTrimmedReasonExceedsMaximumLength_ShouldThrow()
    {
        var reason =
            $" {IpRuleTestFactory.CreateString(IpRule.ReasonMaxLength + 1)} ";

        var action = () => IpRuleTestFactory.Create(reason: reason);

        action.Should().Throw<DomainException>();
    }
}