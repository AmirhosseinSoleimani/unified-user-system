using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Interfaces.Services;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Application.Services.Auditing;
using UnifiedUserSystem.src.Application.Services.Identity;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Business.policies;
using UnifiedUserSystem.src.Business.validators;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Infrastructure.Persistence;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories;
using UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Auditing;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.Infrastructure.Web;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<AuthProtectionOptions>(
    builder.Configuration.GetSection("AuthProtection"));

var authProtectionOptions = builder.Configuration
    .GetSection("AuthProtection")
    .Get<AuthProtectionOptions>() ?? new AuthProtectionOptions();

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
                PermitLimit = authProtectionOptions.AuthRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(authProtectionOptions.AuthRateLimitWindowSeconds),
                QueueLimit = authProtectionOptions.AuthRateLimitQueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.AddPolicy("SensitiveAdminRateLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authProtectionOptions.SensitiveAdminRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(authProtectionOptions.SensitiveAdminRateLimitWindowSeconds),
                QueueLimit = authProtectionOptions.SensitiveAdminRateLimitQueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(t => t.FullName);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UnifiedUserSystem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IClientContext, ClientContext>();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RefreshTokenOptions>(builder.Configuration.GetSection("RefreshToken"));

var jwtOpt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtOpt.Key);

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.MapInboundClaims = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOpt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOpt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, OperationAuthorizationHandler>();

builder.Services.AddScoped<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddScoped<IUserBusiness, UserBusiness>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IRoleOperationRepository, RoleOperationRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<ITemporarySecurityStateStore, MemoryTemporarySecurityStateStore>();
builder.Services.AddScoped<IAuthProtectionService, AuthProtectionService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<ExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRateLimiter();

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

public partial class Program { }