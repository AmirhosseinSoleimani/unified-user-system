using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using UnifiedUserSystem.Api;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.Infrastructure;
using UnifiedUserSystem.src.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<AuthProtectionOptions>(
    builder.Configuration.GetSection("AuthProtection"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<PermissionEvaluationOptions>(
    builder.Configuration.GetSection("PermissionEvaluation"));

var disableSecurityRateLimiting =
    builder.Configuration.GetValue<bool>("DisableSecurityRateLimiting");

var authRateLimitPermitLimit =
    builder.Configuration.GetValue<int?>("AuthProtection:AuthRateLimitPermitLimit")
    ?? builder.Configuration.GetValue<int?>("SecurityRateLimits:Auth:PermitLimit")
    ?? 10;

var authRateLimitWindowSeconds =
    builder.Configuration.GetValue<int?>("AuthProtection:AuthRateLimitWindowSeconds")
    ?? builder.Configuration.GetValue<int?>("SecurityRateLimits:Auth:WindowSeconds")
    ?? 60;

var authRateLimitQueueLimit =
    builder.Configuration.GetValue<int?>("AuthProtection:AuthRateLimitQueueLimit")
    ?? 0;

var sensitiveAdminRateLimitPermitLimit =
    builder.Configuration.GetValue<int?>("AuthProtection:SensitiveAdminRateLimitPermitLimit")
    ?? builder.Configuration.GetValue<int?>("SecurityRateLimits:SensitiveAdmin:PermitLimit")
    ?? 30;

var sensitiveAdminRateLimitWindowSeconds =
    builder.Configuration.GetValue<int?>("AuthProtection:SensitiveAdminRateLimitWindowSeconds")
    ?? builder.Configuration.GetValue<int?>("SecurityRateLimits:SensitiveAdmin:WindowSeconds")
    ?? 60;

var sensitiveAdminRateLimitQueueLimit =
    builder.Configuration.GetValue<int?>("AuthProtection:SensitiveAdminRateLimitQueueLimit")
    ?? 0;

if (!disableSecurityRateLimiting)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, ct) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail("Too many requests. Please try again later."),
                ct);
        };

        options.AddPolicy("AuthRateLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authRateLimitPermitLimit,
                    Window = TimeSpan.FromSeconds(authRateLimitWindowSeconds),
                    QueueLimit = authRateLimitQueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));

        options.AddPolicy("SensitiveAdminRateLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = sensitiveAdminRateLimitPermitLimit,
                    Window = TimeSpan.FromSeconds(sensitiveAdminRateLimitWindowSeconds),
                    QueueLimit = sensitiveAdminRateLimitQueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));
    });
}

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!disableSecurityRateLimiting)
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string GetRateLimitPartitionKey(HttpContext context)
{
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();

    var ipAddress = !string.IsNullOrWhiteSpace(forwardedFor)
        ? forwardedFor
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()
        : context.Connection.RemoteIpAddress?.ToString();

    ipAddress = string.IsNullOrWhiteSpace(ipAddress)
        ? "unknown-ip"
        : ipAddress.Trim().ToLowerInvariant();

    var clientId = context.Request.Headers["X-Client-Id"].ToString();

    if (string.IsNullOrWhiteSpace(clientId))
        return ipAddress;

    return $"{clientId.Trim().ToLowerInvariant()}:{ipAddress}";
}

public partial class Program
{
}