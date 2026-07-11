using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Api.Controllers;

namespace UnifiedUserSystem.UnitTests.Api.Controllers;

public class ProfileControllerTests
{
    [Fact]
    public void GetMyProfile_Should_Have_HttpGetMe_Attribute()
    {
        var method = typeof(ProfileController).GetMethod("GetMyProfile");

        var attr = method!
            .GetCustomAttributes(typeof(HttpGetAttribute), true)
            .Cast<HttpGetAttribute>()
            .Single();

        attr.Template.Should().Be("me");
    }

    [Fact]
    public void GetMyProfile_Should_Have_AuthorizeAttribute()
    {
        var method = typeof(ProfileController).GetMethod("GetMyProfile");

        var attr = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attr.Should().NotBeNull();
    }
}