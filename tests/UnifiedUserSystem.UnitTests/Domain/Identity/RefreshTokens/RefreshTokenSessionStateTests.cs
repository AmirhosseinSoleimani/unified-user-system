
using FluentAssertions;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.RefreshTokens;

[Trait("Category", "Domain")]
[Trait("Feature", "Authentication")]
[Trait("Entity", "RefreshTokenSession")]
public sealed class RefreshTokenSessionStateTests
{

    [Fact]
    public void NewSession_ShouldBeActive()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.IsRevoked.Should().BeFalse();

        session.IsExpired(
            RefreshTokenSessionTestFactory.IssuedAt
            .AddDays(1))
            .Should()
            .BeFalse();


        session.IsActive(
            RefreshTokenSessionTestFactory.IssuedAt
            .AddDays(1))
            .Should()
            .BeTrue();
    }


    [Fact]
    public void IsExpired_WhenNowAfterExpiration_ShouldReturnTrue()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.IsExpired(
            RefreshTokenSessionTestFactory.ExpiresAt
            .AddSeconds(1))
            .Should()
            .BeTrue();
    }


    [Fact]
    public void IsExpired_WhenNowEqualsExpiration_ShouldReturnTrue()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.IsExpired(
            RefreshTokenSessionTestFactory.ExpiresAt)
            .Should()
            .BeTrue();
    }


    [Fact]
    public void ExpiredSession_ShouldNotBeActive()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.IsActive(
            RefreshTokenSessionTestFactory.ExpiresAt)
            .Should()
            .BeFalse();
    }


    [Fact]
    public void Revoke_ShouldMarkSessionRevoked()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var revokeTime =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1);


        var actor =
            Guid.NewGuid();


        session.Revoke(
            revokeTime,
            actor);


        session.IsRevoked.Should().BeTrue();

        session.RevokedAtUtc.Should()
            .Be(revokeTime);


        session.UpdatedAt.Should()
            .Be(revokeTime);

        session.UpdatedByUserId.Should()
            .Be(actor);
    }


    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldBeNoOp()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var first =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1);


        session.Revoke(
            first,
            Guid.NewGuid());


        session.Revoke(
            first.AddHours(1),
            Guid.NewGuid());


        session.RevokedAtUtc.Should()
            .Be(first);
    }


    [Fact]
    public void RevokedSession_ShouldNotBeActive()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.Revoke(
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1),
            null);


        session.IsActive(
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(2))
            .Should()
            .BeFalse();
    }
}
