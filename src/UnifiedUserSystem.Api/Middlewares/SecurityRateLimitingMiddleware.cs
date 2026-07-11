using UnifiedUserSystem.src.Api.RateLimiting;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Contracts.Common;

namespace UnifiedUserSystem.src.Api.Middlewares;

public sealed class SecurityRateLimitingMiddleware : IMiddleware
{
    private readonly ISecurityRateLimitService _rateLimitService;

    public SecurityRateLimitingMiddleware(ISecurityRateLimitService rateLimitService)
    {
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EnableSecurityRateLimitingAttribute>();

        if (metadata is null)
        {
            await next(context);
            return;
        }

        var decision = await _rateLimitService.CheckAsync(
            new SecurityRateLimitContext(
                metadata.PolicyName,
                GetPartitionKey(context)),
            context.RequestAborted);

        if (decision.IsAllowed)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        if (decision.RetryAfter.HasValue)
            context.Response.Headers.RetryAfter = Math.Ceiling(decision.RetryAfter.Value.TotalSeconds).ToString("0");

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(decision.Reason),
            context.RequestAborted);
    }

    private static string GetPartitionKey(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();

        var ipAddress = !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
            : context.Connection.RemoteIpAddress?.ToString();

        ipAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? "unknown-ip"
            : ipAddress.Trim().ToLowerInvariant();

        var clientId = context.Request.Headers["X-Client-Id"].ToString();

        return string.IsNullOrWhiteSpace(clientId)
            ? ipAddress
            : $"{clientId.Trim().ToLowerInvariant()}:{ipAddress}";
    }
}