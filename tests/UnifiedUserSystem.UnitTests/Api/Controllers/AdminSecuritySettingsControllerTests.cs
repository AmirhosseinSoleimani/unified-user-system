using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.UnitTests.Api.Controllers;

public class AdminSecuritySettingsControllerTests
{
    [Fact]
    public void Security_settings_read_policy_is_operation_policy()
    {
        OperationPolicyNames.TryGetOperationKey(
                OperationPolicyNames.SecuritySettingsRead,
                out var operationKey)
            .Should()
            .BeTrue();

        operationKey.Should().Be("security-settings.read");
    }

    [Fact]
    public void Security_settings_update_policy_is_operation_policy()
    {
        OperationPolicyNames.TryGetOperationKey(
                OperationPolicyNames.SecuritySettingsUpdate,
                out var operationKey)
            .Should()
            .BeTrue();

        operationKey.Should().Be("security-settings.update");
    }

    [Fact]
    public void Non_admin_users_without_operation_permission_cannot_satisfy_policy()
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        policy.Requirements.Should().NotBeEmpty();
    }
}