using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using UnifiedUserSystem.Api;
using UnifiedUserSystem.Infrastructure;
using UnifiedUserSystem.src.Api.Localization;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Infrastructure.Security;
using MfaOptions = UnifiedUserSystem.src.Application.Options.MfaOptions;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("========================================");
Console.WriteLine($"Environment : {builder.Environment.EnvironmentName}");
Console.WriteLine($"ContentRoot : {builder.Environment.ContentRootPath}");
Console.WriteLine($"Application : {typeof(Program).Assembly.Location}");
Console.WriteLine("========================================");

var configurationRoot = (IConfigurationRoot)builder.Configuration;

foreach (var provider in configurationRoot.Providers)
{
    if (!provider.TryGet(
            "ConnectionStrings:Default",
            out var connectionString) ||
        string.IsNullOrWhiteSpace(connectionString))
    {
        continue;
    }

    try
    {
        var cs = new NpgsqlConnectionStringBuilder(connectionString);

        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Provider : {provider}");
        Console.WriteLine($"Host     : {cs.Host}");
        Console.WriteLine($"Port     : {cs.Port}");
        Console.WriteLine($"Database : {cs.Database}");
        Console.WriteLine($"Username : {cs.Username}");
    }
    catch
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Provider : {provider}");
        Console.WriteLine("Connection string could not be parsed.");
    }
}

Console.WriteLine("========================================");

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<AuthProtectionOptions>(
    builder.Configuration.GetSection("AuthProtection"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<PermissionEvaluationOptions>(
    builder.Configuration.GetSection("PermissionEvaluation"));

builder.Services.Configure<SecuritySettingsDefaultsOptions>(
    builder.Configuration.GetSection("SecuritySettingsDefaults"));

builder.Services.Configure<SecurityRuntimeOptions>(
    builder.Configuration.GetSection("SecurityRuntime"));

builder.Services.Configure<MfaOptions>(
    builder.Configuration.GetSection("Mfa"));

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
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await WriteAuthenticationFailureAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    MessageCodes.Unauthorized);
            },
            OnForbidden = context => WriteAuthenticationFailureAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                MessageCodes.Forbidden)
        };
    });

builder.Services.AddMemoryCache();

static async Task WriteAuthenticationFailureAsync(
    HttpContext httpContext,
    int statusCode,
    string code)
{
    if (httpContext.Response.HasStarted)
        return;

    var resolver = httpContext.RequestServices
        .GetRequiredService<IRequestLocaleResolver>();
    var localizer = httpContext.RequestServices
        .GetRequiredService<IBusinessMessageLocalizer>();
    var locale = resolver.Resolve(httpContext);
    var message = localizer.Get(code, locale);

    httpContext.Response.StatusCode = statusCode;
    httpContext.Response.ContentType = "application/json";
    httpContext.Response.Headers["Content-Language"] = locale;

    await httpContext.Response.WriteAsJsonAsync(
        ApiResponse<object>.Fail(
            message,
            code: code,
            traceId: httpContext.TraceIdentifier));
}

var app = builder.Build();


app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();
}

app.UseMiddleware<SecurityRateLimitingMiddleware>();
app.UseMiddleware<IpAccessControlMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program
{
}