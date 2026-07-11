using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.UnitTests.Api.Authorization;

public class OperationPolicyProviderTests
{
    [Fact]
    public async Task OperationPolicyProvider_GetPolicyAsync_WithOperationPolicy_ShouldReturnPolicyWithOperationRequirement()
    {
        var sut = CreateSut();

        var policy = await sut.GetPolicyAsync(OperationPolicyNames.UsersRead);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<OperationRequirement>().Single().OperationKey.Should().Be("users.read");
    }

    [Fact]
    public async Task OperationPolicyProvider_GetPolicyAsync_WithOperationPolicy_ShouldRequireAuthenticatedUser()
    {
        var sut = CreateSut();

        var policy = await sut.GetPolicyAsync(OperationPolicyNames.UsersRead);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Should().ContainSingle();
    }

    [Fact]
    public async Task OperationPolicyProvider_GetPolicyAsync_WithNonOperationPolicy_ShouldDelegateToFallbackProvider()
    {
        var options = new AuthorizationOptions();
        options.AddPolicy("named-policy", builder => builder.RequireClaim("scope", "test"));

        var sut = CreateSut(options);

        var policy = await sut.GetPolicyAsync("named-policy");

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<ClaimsAuthorizationRequirement>()
            .Single()
            .ClaimType
            .Should()
            .Be("scope");
    }

    [Fact]
    public async Task OperationPolicyProvider_GetDefaultPolicyAsync_ShouldDelegateToFallbackProvider()
    {
        var sut = CreateSut();

        var policy = await sut.GetDefaultPolicyAsync();

        policy.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Should().ContainSingle();
    }

    [Fact]
    public async Task OperationPolicyProvider_GetFallbackPolicyAsync_ShouldDelegateToFallbackProvider()
    {
        var options = new AuthorizationOptions
        {
            FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireClaim("fallback", "true")
                .Build()
        };

        var sut = CreateSut(options);

        var policy = await sut.GetFallbackPolicyAsync();

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<ClaimsAuthorizationRequirement>()
            .Single()
            .ClaimType
            .Should()
            .Be("fallback");
    }

    private static OperationPolicyProvider CreateSut(AuthorizationOptions? options = null)
    {
        return new OperationPolicyProvider(Options.Create(options ?? new AuthorizationOptions()));
    }
}
