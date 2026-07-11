
using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.RefreshTokens;

[Trait("Category", "Security")]
[Trait("Feature", "Authentication")]
[Trait("Entity", "RefreshTokenSession")]
public sealed class RefreshTokenSessionSecurityTests
{

    [Fact]
    public void Rotate_ShouldStoreReplacementSessionId()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var replacementId =
            Guid.NewGuid();


        var rotateTime =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1);


        session.Rotate(
            replacementId,
            rotateTime,
            null);


        session.ReplacedBySessionId.Should()
            .Be(replacementId);


        session.RevokedAtUtc.Should()
            .Be(rotateTime);
    }


    [Fact]
    public void Rotate_WithEmptyReplacementId_ShouldThrow()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var action =
            () => session.Rotate(
                Guid.Empty,
                RefreshTokenSessionTestFactory.IssuedAt,
                null);


        action.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Rotate_CalledTwice_ShouldThrow()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        session.Rotate(
            Guid.NewGuid(),
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1),
            null);


        var action =
            () => session.Rotate(
                Guid.NewGuid(),
                RefreshTokenSessionTestFactory.IssuedAt
                .AddHours(2),
                null);


        action.Should()
            .Throw<DomainException>()
            .WithMessage("*already been rotated*");
    }


    [Fact]
    public void MarkReuseDetected_ShouldSetDetectionTime()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var detectedAt =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(2);


        session.MarkReuseDetected(
            detectedAt,
            null);


        session.ReuseDetectedAtUtc.Should()
            .Be(detectedAt);
    }


    [Fact]
    public void MarkReuseDetected_MultipleTimes_ShouldKeepFirstDetection()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var first =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1);


        var second = first.AddHours(1);


        session.MarkReuseDetected(
            first,
            null);


        session.MarkReuseDetected(
            second,
            null);


        session.ReuseDetectedAtUtc.Should()
            .Be(first);
    }


    [Fact]
    public void Rotate_ShouldPreserveReuseDetection()
    {
        var session =
            RefreshTokenSessionTestFactory.Create();


        var reuseTime =
            RefreshTokenSessionTestFactory.IssuedAt
            .AddHours(1);


        session.MarkReuseDetected(
            reuseTime,
            null);


        session.Rotate(
            Guid.NewGuid(),
            reuseTime.AddHours(1),
            null);


        session.ReuseDetectedAtUtc.Should()
            .Be(reuseTime);
    }
}
