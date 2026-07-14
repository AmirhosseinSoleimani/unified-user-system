using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Api.Middlewares;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldReturnPersianProblemDetails_WhenLanguageIsFa()
    {
        var cache = CreateCache();
        var middleware = new ExceptionHandlingMiddleware(cache.Object);
        var context = CreateHttpContext("fa");

        await middleware.InvokeAsync(
            context,
            _ => throw new DomainException("Email is invalid."));

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var json = await ReadResponseJsonAsync(context);
        json.RootElement.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("errorCode").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("language").GetString().Should().Be("fa");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnEnglishProblemDetails_WhenLanguageIsEn()
    {
        var cache = CreateCache();
        var middleware = new ExceptionHandlingMiddleware(cache.Object);
        var context = CreateHttpContext("en");

        await middleware.InvokeAsync(
            context,
            _ => throw new KeyNotFoundException("User was not found."));

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var json = await ReadResponseJsonAsync(context);
        json.RootElement.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("errorCode").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("detail").GetString().Should().Be("Resource was not found.");
        json.RootElement.GetProperty("language").GetString().Should().Be("en");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipSwaggerRequests()
    {
        var cache = CreateCache();
        var middleware = new ExceptionHandlingMiddleware(cache.Object);
        var context = CreateHttpContext("fa");
        context.Request.Path = "/swagger/index.html";
        var nextWasCalled = false;

        await middleware.InvokeAsync(
            context,
            _ =>
            {
                nextWasCalled = true;
                return Task.CompletedTask;
            });

        nextWasCalled.Should().BeTrue();
        cache.Verify(
            x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<LocalizedMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<ILocalizedMessageCache> CreateCache()
    {
        var cache = new Mock<ILocalizedMessageCache>();

        cache.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<LocalizedMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, LocalizedMessage fallback, CancellationToken _) =>
            {
                return key switch
                {
                    "domain_error.title" => new LocalizedMessage("Domain error", "خطای دامنه"),
                    "domain_error.detail" => new LocalizedMessage("Domain request is invalid.", "درخواست دامنه نامعتبر است."),
                    "not_found.title" => new LocalizedMessage("Not found", "یافت نشد"),
                    "not_found.detail" => new LocalizedMessage("Resource was not found.", "منبع پیدا نشد."),
                    _ => fallback
                };
            });

        return cache;
    }

    private static DefaultHttpContext CreateHttpContext(string language)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Language"] = language;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var response = await reader.ReadToEndAsync();
        return JsonDocument.Parse(response);
    }
}

