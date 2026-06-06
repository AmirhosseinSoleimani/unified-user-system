using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using UnifiedUserSystem.src.Api.Controllers;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Api.Controllers;

public class RoleAndUserRouteContractTests
{
    [Fact]
    public void RoleController_ShouldNotExposeDuplicateUserRoleRemovalEndpoint()
    {
        var routes = GetRoutes(typeof(RoleController));

        routes.Should().NotContain(x =>
            x.HttpMethod == "POST" &&
            x.Template == "remove");
    }

    [Fact]
    public void UsersController_ShouldExposeUserRoleRemovalEndpoint()
    {
        var routes = GetRoutes(typeof(UsersController));

        routes.Should().Contain(x =>
            x.HttpMethod == "DELETE" &&
            x.Template == "{userId:guid}/roles/{roleId:int}");
    }

    [Fact]
    public void RoleController_ShouldExposeRoleOperationRemovalEndpoint()
    {
        var routes = GetRoutes(typeof(RoleController));

        routes.Should().Contain(x =>
            x.HttpMethod == "DELETE" &&
            x.Template == "{roleId:int}/operations/{operationId:guid}");
    }

    private static IReadOnlyList<RouteInfo> GetRoutes(Type controllerType)
    {
        return controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .SelectMany(method => method
                .GetCustomAttributes<HttpMethodAttribute>()
                .Select(attribute => new RouteInfo(
                    attribute.HttpMethods.Single(),
                    attribute.Template ?? string.Empty)))
            .ToArray();
    }

    private sealed record RouteInfo(string HttpMethod, string Template);
}