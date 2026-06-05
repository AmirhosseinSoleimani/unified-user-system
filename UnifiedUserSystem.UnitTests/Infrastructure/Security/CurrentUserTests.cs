using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnifiedUserSystem.src.Infrastructure.Security;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Security;

public class CurrentUserTests
{
    [Fact]
    public void UserId_Should_ReturnNull_When_HttpContextIsMissing()
    {
        var accessor = new HttpContextAccessor();
        var currentUser = new CurrentUser(accessor);

        Assert.Null(currentUser.UserId);
        Assert.False(currentUser.IsAuthenticated);
    }

    [Fact]
    public void UserId_Should_ReturnNameIdentifierClaim_When_Authenticated()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var currentUser = new CurrentUser(accessor);

        Assert.Equal(userId, currentUser.UserId);
        Assert.True(currentUser.IsAuthenticated);
    }

    [Theory]
    [InlineData("sub")]
    [InlineData("uid")]
    [InlineData("userId")]
    public void UserId_Should_ReturnSupportedCustomUserIdClaim_When_NameIdentifierIsMissing(string claimType)
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new Claim(claimType, userId.ToString()));
        var currentUser = new CurrentUser(accessor);

        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UserId_Should_ReturnNull_When_UserIdClaimIsInvalid()
    {
        var accessor = CreateAccessor(new Claim("sub", "not-a-guid"));
        var currentUser = new CurrentUser(accessor);

        Assert.Null(currentUser.UserId);
    }

    private static HttpContextAccessor CreateAccessor(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        return new HttpContextAccessor { HttpContext = context };
    }
}