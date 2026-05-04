using FluentAssertions;
using Microsoft.AspNetCore.Http;
using UnifiedUserSystem.src.Api.Middlewares;

namespace UnifiedUserSystem.UnitTests.Api.Middlewares
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task Should_Map_KeyNotFoundException_To_404()
        {
            var middleware = new ExceptionHandlingMiddleware();

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = _ => throw new KeyNotFoundException("not found");

            await middleware.InvokeAsync(context, next);

            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

            body.Should().Contain("Not found");
        }
    }
}
