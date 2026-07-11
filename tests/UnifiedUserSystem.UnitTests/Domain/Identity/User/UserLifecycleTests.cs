using FluentAssertions;


namespace UnifiedUserSystem.UnitTests.Domain.Identity.User;

[Trait("Category", "Domain")]
[Trait("Entity", "User")]
public sealed class UserLifecycleTests
{
    [Fact]
    public void NewUser_ShouldBeActive()
    {
        var user = UserTestFactory.Create();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenUserActive_ShouldDeactivateUser()
    {
        var user = UserTestFactory.Create();
        var deactivatedAt = UserTestFactory.CreatedAt.AddMinutes(1);
        var actorId = Guid.NewGuid();

        user.Deactivate(deactivatedAt, actorId);

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(deactivatedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Deactivate_WithoutActor_ShouldUseUserIdForAudit()
    {
        var user = UserTestFactory.Create();

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            actorUserId: null);

        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            UserTestFactory.ActorUserId);

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Activate_WhenUserInactive_ShouldActivateUser()
    {
        var user = UserTestFactory.Create();

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            UserTestFactory.ActorUserId);

        var activatedAt = UserTestFactory.CreatedAt.AddMinutes(2);
        var actorId = Guid.NewGuid();

        user.Activate(activatedAt, actorId);

        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().Be(activatedAt);
        user.UpdatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public void Activate_WithoutActor_ShouldUseUserIdForAudit()
    {
        var user = UserTestFactory.Create();

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.Activate(
            UserTestFactory.CreatedAt.AddMinutes(2),
            actorUserId: null);

        user.UpdatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldBeNoOp()
    {
        var user = UserTestFactory.Create();

        var originalUpdatedAt = user.UpdatedAt;
        var originalUpdatedBy = user.UpdatedByUserId;

        user.Activate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.UpdatedByUserId.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void DeactivateThenActivate_ShouldPreserveIdentityInformation()
    {
        var user = UserTestFactory.Create();

        user.Deactivate(
            UserTestFactory.CreatedAt.AddMinutes(1),
            Guid.NewGuid());

        user.Activate(
            UserTestFactory.CreatedAt.AddMinutes(2),
            Guid.NewGuid());

        user.Email.Should().Be("user@example.com");
        user.Username.Should().Be("test.user");
        user.FirstName.Should().Be("Amirhossein");
        user.LastName.Should().Be("Soleimani");
        user.PhoneNumber.Should().Be("09123456789");
        user.PasswordHash.Should().Be("valid-password-hash");
    }
}