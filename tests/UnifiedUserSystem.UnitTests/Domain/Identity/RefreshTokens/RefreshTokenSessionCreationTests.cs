using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.RefreshTokens;

[Trait("Category", "Domain")]
[Trait("Feature", "Authentication")]
[Trait("Entity", "RefreshTokenSession")]
public sealed class RefreshTokenSessionCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSession()
    {
        var session = RefreshTokenSessionTestFactory.Create();

        session.Id.Should().NotBeEmpty();

        session.UserId.Should()
            .Be(RefreshTokenSessionTestFactory.UserId);

        session.RefreshTokenHash.Should()
            .Be("refresh-token-hash");

        session.IssuedAtUtc.Should()
            .Be(RefreshTokenSessionTestFactory.IssuedAt);

        session.ExpiresAtUtc.Should()
            .Be(RefreshTokenSessionTestFactory.ExpiresAt);

        session.RevokedAtUtc.Should().BeNull();
        session.ReplacedBySessionId.Should().BeNull();
        session.ReuseDetectedAtUtc.Should().BeNull();
    }


    [Fact]
    public void Create_ShouldInitializeAudit()
    {
        var session = RefreshTokenSessionTestFactory.Create(
            actorUserId:
                RefreshTokenSessionTestFactory.ActorUserId);


        session.CreatedAt.Should()
            .Be(RefreshTokenSessionTestFactory.IssuedAt);

        session.UpdatedAt.Should()
            .Be(RefreshTokenSessionTestFactory.IssuedAt);


        session.CreatedByUserId.Should()
            .Be(RefreshTokenSessionTestFactory.ActorUserId);

        session.UpdatedByUserId.Should()
            .Be(RefreshTokenSessionTestFactory.ActorUserId);
    }


    [Fact]
    public void Create_WithoutActor_ShouldUseUserId()
    {
        var session =
            RefreshTokenSessionTestFactory.Create(
                actorUserId: null);


        session.CreatedByUserId.Should()
            .Be(RefreshTokenSessionTestFactory.UserId);

        session.UpdatedByUserId.Should()
            .Be(RefreshTokenSessionTestFactory.UserId);
    }


    [Fact]
    public void Create_WhenUserIdEmpty_ShouldThrow()
    {
        var action =
            () => RefreshTokenSessionTestFactory.Create(
                userId: Guid.Empty);


        action.Should()
            .Throw<DomainException>()
            .WithMessage("*UserId is invalid*");
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenRefreshTokenHashEmpty_ShouldThrow(
        string? hash)
    {
        var action =
            () => RefreshTokenSessionTestFactory.Create(
                refreshTokenHash: hash!);


        action.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Create_WhenHashTooLong_ShouldThrow()
    {
        var hash =
            RefreshTokenSessionTestFactory.CreateString(
                RefreshTokenSession.RefreshTokenHashMaxLength + 1);


        var action =
            () => RefreshTokenSessionTestFactory.Create(
                refreshTokenHash: hash);


        action.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Create_WhenExpirationBeforeIssue_ShouldThrow()
    {
        var action =
            () => RefreshTokenSessionTestFactory.Create(
                expiresAtUtc:
                    RefreshTokenSessionTestFactory.IssuedAt
                    .AddMinutes(-1));


        action.Should()
            .Throw<DomainException>()
            .WithMessage("*ExpiresAtUtc must be after IssuedAtUtc*");
    }


    [Fact]
    public void Create_WhenExpirationEqualsIssue_ShouldThrow()
    {
        var action =
            () => RefreshTokenSessionTestFactory.Create(
                expiresAtUtc:
                    RefreshTokenSessionTestFactory.IssuedAt);


        action.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Create_ShouldNormalizeOptionalFields()
    {
        var session =
            RefreshTokenSessionTestFactory.Create(
                deviceName: " Chrome ",
                userAgent: " Browser ",
                ipAddress: " 127.0.0.1 ",
                clientId: " web "
            );


        session.DeviceName.Should()
            .Be("Chrome");

        session.UserAgent.Should()
            .Be("Browser");

        session.IpAddress.Should()
            .Be("127.0.0.1");

        session.ClientId.Should()
            .Be("web");
    }


    [Fact]
    public void Create_WithEmptyOptionalFields_ShouldStoreNull()
    {
        var session =
            RefreshTokenSessionTestFactory.Create(
                deviceName: " ",
                userAgent: "",
                ipAddress: null,
                clientId: " "
            );


        session.DeviceName.Should().BeNull();
        session.UserAgent.Should().BeNull();
        session.IpAddress.Should().BeNull();
        session.ClientId.Should().BeNull();
    }


    [Fact]
    public void Create_WhenDeviceNameTooLong_ShouldThrow()
    {
        var value =
            RefreshTokenSessionTestFactory.CreateString(
                RefreshTokenSession.DeviceNameMaxLength + 1);


        var action =
            () => RefreshTokenSessionTestFactory.Create(
                deviceName: value);


        action.Should()
            .Throw<DomainException>();
    }
}
