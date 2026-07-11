
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Security.IpRules;

[Trait("Category", "Domain")]
[Trait("Entity", "IpRule")]
public sealed class IpRuleDisableTests
{
    [Fact]
    public void Disable_WhenRuleActive_ShouldDeactivateRule()
    {
        var rule = IpRuleTestFactory.Create();

        var disabledAt = IpRuleTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        rule.Disable(
            nowUtc: disabledAt,
            actorUserId: actorId);

        rule.IsActive.Should().BeFalse();
        rule.DisabledAt.Should().Be(disabledAt);

        rule.UpdatedAt.Should().Be(disabledAt);
        rule.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Disable_WithoutActor_ShouldSetAuditActorToNull()
    {
        var rule = IpRuleTestFactory.Create(
            actorUserId: IpRuleTestFactory.ActorUserId);

        rule.Disable(
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        rule.IsActive.Should().BeFalse();
        rule.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Disable_ShouldPreserveRuleData()
    {
        var rule = IpRuleTestFactory.Create(
            ipAddressOrCidr: "192.168.1.0/24",
            reason: "Blocked network");

        rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        rule.IpAddressOrCidr.Should().Be("192.168.1.0/24");
        rule.NormalizedIpAddressOrCidr.Should()
            .Be("192.168.1.0/24");

        rule.RuleType.Should()
            .Be(UnifiedUserSystem.src.Domain.Security.Enums.IpRuleType.Block);

        rule.Reason.Should().Be("Blocked network");
    }

    [Fact]
    public void Disable_WhenAlreadyDisabled_ShouldThrow()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(1),
            IpRuleTestFactory.ActorUserId);

        var action = () => rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*IP rule is already disabled*");
    }

    [Fact]
    public void Disable_WhenAlreadyDisabled_ShouldNotChangeState()
    {
        var rule = IpRuleTestFactory.Create();

        var firstDisabledAt =
            IpRuleTestFactory.CreatedAt.AddMinutes(1);

        var firstActorId = Guid.NewGuid();

        rule.Disable(firstDisabledAt, firstActorId);

        var originalUpdatedAt = rule.UpdatedAt;
        var originalUpdatedBy = rule.UpdatedByUserId;
        var originalDisabledAt = rule.DisabledAt;

        var action = () => rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        action.Should().Throw<DomainException>();

        rule.IsActive.Should().BeFalse();
        rule.DisabledAt.Should().Be(originalDisabledAt);
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
        rule.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Update_CanReactivateDisabledRule()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        rule.Update(
            ipAddressOrCidr: rule.IpAddressOrCidr,
            ruleType: rule.RuleType,
            reason: rule.Reason,
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: Guid.NewGuid());

        rule.IsActive.Should().BeTrue();
        rule.DisabledAt.Should().BeNull();
    }

    [Fact]
    public void ReactivatedRule_CanBeDisabledAgain()
    {
        var rule = IpRuleTestFactory.Create();

        rule.Disable(
            IpRuleTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        rule.Update(
            ipAddressOrCidr: rule.IpAddressOrCidr,
            ruleType: rule.RuleType,
            reason: rule.Reason,
            isActive: true,
            nowUtc: IpRuleTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: Guid.NewGuid());

        var secondDisabledAt =
            IpRuleTestFactory.CreatedAt.AddMinutes(3);

        rule.Disable(
            secondDisabledAt,
            Guid.NewGuid());

        rule.IsActive.Should().BeFalse();
        rule.DisabledAt.Should().Be(secondDisabledAt);
    }
}
