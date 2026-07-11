using UnifiedUserSystem.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Contracts.Common;

namespace UnifiedUserSystem.src.Api.Middlewares;

public sealed class IpAccessControlMiddleware : IMiddleware
{
    private readonly IIpAccessPolicyService _ipAccessPolicyService;
    private readonly ICurrentUser _currentUser;

    public IpAccessControlMiddleware(
        IIpAccessPolicyService ipAccessPolicyService,
        ICurrentUser currentUser)
    {
        _ipAccessPolicyService = ipAccessPolicyService;
        _currentUser = currentUser;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            await next(context);
            return;
        }

        var decision = await _ipAccessPolicyService.CheckAsync(
            ipAddress,
            context.Request.Path.Value,
            context.Request.Method,
            _currentUser.UserId,
            context.RequestAborted);

        if (decision.IsAllowed)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(decision.Reason ?? "IP address is blocked."),
            context.RequestAborted);
    }
}
