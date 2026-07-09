using System.Net;
using Microsoft.AspNetCore.Http;
using UnifiedUserSystem.src.Infrastructure.Web;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Web;

public class ClientContextTests
{
    [Fact]
    public void Properties_Should_ReturnTrimmedHeaderValues_When_HeadersExist()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Device-Name"] = " phone ";
        context.Request.Headers["User-Agent"] = " browser ";
        context.Request.Headers["X-Client-Id"] = " client ";

        var clientContext = new ClientContext(new HttpContextAccessor { HttpContext = context });

        Assert.Equal("phone", clientContext.DeviceName);
        Assert.Equal("browser", clientContext.UserAgent);
        Assert.Equal("client", clientContext.ClientId);
    }

    [Fact]
    public void IpAddress_Should_ReturnFirstForwardedForValue_When_HeaderExists()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = " 10.0.0.1, 10.0.0.2 ";

        var clientContext = new ClientContext(new HttpContextAccessor { HttpContext = context });

        Assert.Equal("10.0.0.1", clientContext.IpAddress);
    }

    [Fact]
    public void IpAddress_Should_ReturnRemoteIpAddress_When_ForwardedForIsMissing()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var clientContext = new ClientContext(new HttpContextAccessor { HttpContext = context });

        Assert.Equal("127.0.0.1", clientContext.IpAddress);
    }

    [Fact]
    public void Properties_Should_ReturnNull_When_HeadersAreMissingOrWhitespace()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Device-Name"] = " ";

        var clientContext = new ClientContext(new HttpContextAccessor { HttpContext = context });

        Assert.Null(clientContext.DeviceName);
        Assert.Null(clientContext.UserAgent);
        Assert.Null(clientContext.ClientId);
    }
}