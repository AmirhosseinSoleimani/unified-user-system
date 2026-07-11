
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;

namespace UnifiedUserSystem.UnitTests.Domain.Security.IpRules;

[Trait("Category", "Domain")]
[Trait("Entity", "IpRule")]
public sealed class IpRuleUpdateTests
{
    [Fact]
    public void Update_WithValidData_ShouldUpdateRule()
    {
        var rule = IpRuleTestFactory.Create();

        var updatedAt = IpRuleTestFactory.CreatedAt.AddMinutes(5);
        var actorId = Guid.NewGuid();

        rule.Update(
            ipAddressOrCidr: "10.0.0.0/8",
            ruleType: IpRuleType.Allow,
            reason: "Trusted internal network",
            isActive: true,
            nowUtc: updatedAt,
            actorUserId: actorId);

        rule.IpAddressOrCidr.Should().Be("10.0.0.0/8");
        rule.NormalizedIpAddressOrCidr.Should().Be("10.0.0.0/8");
        rule.RuleType.Should().Be(IpRuleType.Allow);
        rule.Reason.Should().Be("Trusted internal network");

        rule.IsActive.Should().BeTrue();
        rule.DisabledAt.Should().BeNull();

        rule.UpdatedAt.Should().Be(updatedAt);
        rule.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Update_ShouldNormalizeIpAddress()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Update(
            ipAddressOrCidr:
                "2001:0DB8:0000:0000:0000:0000:0000:0001",
            ruleType: IpRuleType.Restrict,
            reason: null,
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        rule.IpAddressOrCidr.Should().Be("2001:db8::1");
        rule.NormalizedIpAddressOrCidr.Should().Be("2001:db8::1");
    }

    [Fact]
    public void Update_ShouldTrimReason()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: IpRuleType.Restrict,
            reason: " Temporary restriction ",
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        rule.Reason.Should().Be("Temporary restriction");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenReasonMissing_ShouldSetReasonNull(
        string? reason)
    {
        var rule = IpRuleTestFactory.Create();

        rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: IpRuleType.Allow,
            reason: reason,
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        rule.Reason.Should().BeNull();
    }

    [Fact]
    public void Update_WhenActiveRuleBecomesInactive_ShouldSetDisabledAt()
    {
        var rule = IpRuleTestFactory.Create();
        var disabledAt = IpRuleTestFactory.CreatedAt.AddMinutes(5);

        rule.Update(
            ipAddressOrCidr: rule.IpAddressOrCidr,
            ruleType: rule.RuleType,
            reason: rule.Reason,
            isActive: false,
            nowUtc: disabledAt,
            actorUserId: IpRuleTestFactory.ActorUserId);

        rule.IsActive.Should().BeFalse();
        rule.DisabledAt.Should().Be(disabledAt);
        rule.UpdatedAt.Should().Be(disabledAt);
    }

    [Fact]
    public void Update_WhenInactiveRuleBecomesActive_ShouldClearDisabledAt()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(1),
            IpRuleTestFactory.ActorUserId);

        rule.DisabledAt.Should().NotBeNull();

        var activatedAt = IpRuleTestFactory.CreatedAt.AddMinutes(2);

        rule.Update(
            ipAddressOrCidr: rule.IpAddressOrCidr,
            ruleType: rule.RuleType,
            reason: rule.Reason,
            isActive: true,
            nowUtc: activatedAt,
            actorUserId: IpRuleTestFactory.ActorUserId);

        rule.IsActive.Should().BeTrue();
        rule.DisabledAt.Should().BeNull();
        rule.UpdatedAt.Should().Be(activatedAt);
    }

    [Fact]
    public void Update_WhenRuleRemainsInactive_ShouldPreserveOriginalDisabledAt()
    {
        var rule = IpRuleTestFactory.Create();

        var originalDisabledAt =
            IpRuleTestFactory.CreatedAt.AddMinutes(1);

        rule.Disable(
            originalDisabledAt,
            IpRuleTestFactory.ActorUserId);

        var updatedAt = IpRuleTestFactory.CreatedAt.AddMinutes(2);

        rule.Update(
            ipAddressOrCidr: "10.0.0.0/8",
            ruleType: IpRuleType.Restrict,
            reason: "Updated while disabled",
            isActive: false,
            nowUtc: updatedAt,
            actorUserId: Guid.NewGuid());

        rule.IsActive.Should().BeFalse();
        rule.DisabledAt.Should().Be(originalDisabledAt);
        rule.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Update_WhenRuleRemainsActive_ShouldKeepDisabledAtNull()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: IpRuleType.Allow,
            reason: "Updated rule",
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        rule.IsActive.Should().BeTrue();
        rule.DisabledAt.Should().BeNull();
    }

    [Fact]
    public void Update_WithSameValues_ShouldStillTouchAudit()
    {
        var rule = IpRuleTestFactory.Create();

        var updatedAt = IpRuleTestFactory.CreatedAt.AddMinutes(5);
        var actorId = Guid.NewGuid();

        rule.Update(
            ipAddressOrCidr: rule.IpAddressOrCidr,
            ruleType: rule.RuleType,
            reason: rule.Reason,
            isActive: rule.IsActive,
            nowUtc: updatedAt,
            actorUserId: actorId);

        rule.UpdatedAt.Should().Be(updatedAt);
        rule.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Update_WithoutActor_ShouldSetUpdatedActorToNull()
    {
        var rule = IpRuleTestFactory.Create(
            actorUserId: IpRuleTestFactory.ActorUserId);

        rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: IpRuleType.Allow,
            reason: "Changed",
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        rule.UpdatedByUserId.Should().BeNull();

        rule.CreatedByUserId.Should()
            .Be(IpRuleTestFactory.ActorUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("192.168.1.1/33")]
    [InlineData("2001:db8::1/129")]
    public void Update_WhenIpAddressInvalid_ShouldThrow(
        string ipAddressOrCidr)
    {
        var rule = IpRuleTestFactory.Create();

        var action = () => rule.Update(
            ipAddressOrCidr: ipAddressOrCidr,
            ruleType: IpRuleType.Allow,
            reason: "Updated reason",
            isActive: false,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_WhenIpAddressInvalid_ShouldNotChangeState()
    {
        var rule = IpRuleTestFactory.Create(
            actorUserId: IpRuleTestFactory.ActorUserId);

        var originalIp = rule.IpAddressOrCidr;
        var originalNormalizedIp = rule.NormalizedIpAddressOrCidr;
        var originalRuleType = rule.RuleType;
        var originalReason = rule.Reason;
        var originalIsActive = rule.IsActive;
        var originalDisabledAt = rule.DisabledAt;
        var originalUpdatedAt = rule.UpdatedAt;
        var originalUpdatedBy = rule.UpdatedByUserId;

        var action = () => rule.Update(
            ipAddressOrCidr: "invalid",
            ruleType: IpRuleType.Allow,
            reason: "Changed reason",
            isActive: false,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();

        rule.IpAddressOrCidr.Should().Be(originalIp);
        rule.NormalizedIpAddressOrCidr.Should()
            .Be(originalNormalizedIp);

        rule.RuleType.Should().Be(originalRuleType);
        rule.Reason.Should().Be(originalReason);
        rule.IsActive.Should().Be(originalIsActive);
        rule.DisabledAt.Should().Be(originalDisabledAt);
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
        rule.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Update_WhenRuleTypeInvalid_ShouldNotChangeState()
    {
        var rule = IpRuleTestFactory.Create();

        var originalIp = rule.IpAddressOrCidr;
        var originalRuleType = rule.RuleType;
        var originalReason = rule.Reason;
        var originalUpdatedAt = rule.UpdatedAt;

        var action = () => rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: (IpRuleType)999,
            reason: "Changed reason",
            isActive: false,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*IP rule type is invalid*");

        rule.IpAddressOrCidr.Should().Be(originalIp);
        rule.RuleType.Should().Be(originalRuleType);
        rule.Reason.Should().Be(originalReason);
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Update_WhenReasonTooLong_ShouldNotChangeState()
    {
        var rule = IpRuleTestFactory.Create();

        var originalIp = rule.IpAddressOrCidr;
        var originalRuleType = rule.RuleType;
        var originalReason = rule.Reason;
        var originalIsActive = rule.IsActive;
        var originalUpdatedAt = rule.UpdatedAt;

        var tooLongReason = IpRuleTestFactory.CreateString(
            IpRule.ReasonMaxLength + 1);

        var action = () => rule.Update(
            ipAddressOrCidr: "10.0.0.1",
            ruleType: IpRuleType.Allow,
            reason: tooLongReason,
            isActive: false,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: Guid.NewGuid());

        action.Should().Throw<DomainException>();

        rule.IpAddressOrCidr.Should().Be(originalIp);
        rule.RuleType.Should().Be(originalRuleType);
        rule.Reason.Should().Be(originalReason);
        rule.IsActive.Should().Be(originalIsActive);
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
    }
}
