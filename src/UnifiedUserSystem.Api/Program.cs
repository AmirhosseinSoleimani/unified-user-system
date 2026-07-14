using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UnifiedUserSystem.Api;
using UnifiedUserSystem.Infrastructure;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystemsrc.src.Application.Options;
using MfaOptions = UnifiedUserSystem.src.Application.Options.MfaOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ApplicationMetadataDefaultsOptions>(
    builder.Configuration.GetSection("ApplicationMetadata"));

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
    });

builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<SecurityRateLimitingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IpAccessControlMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program
{
}