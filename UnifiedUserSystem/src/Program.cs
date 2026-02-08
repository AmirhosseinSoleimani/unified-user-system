using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Api.Middlewares;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Services;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Business.policies;
using UnifiedUserSystem.src.Business.validators;
using UnifiedUserSystem.src.Infrastructure.Persistence;
using UnifiedUserSystem.src.Infrastructure.Repositories;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
    {
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
                new OpenApiSecurityScheme {Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"} },
                Array.Empty<string>()
            }
        });
    });

// DbContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));



// Jwt options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOpt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtOpt.Key);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
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

// Authorization OP:...
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
builder.Services.AddScoped <IAuthorizationHandler, OperationAuthorizationHandler>();


// Business
builder.Services.AddScoped<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddScoped<IUserBusiness, UserBusiness>();

// Repositories + Uow
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IRoleOperationRepository, RoleOperationRepository>();

//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddScoped<IProductUserRepository, ProductUserRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Security helpers
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Aplication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();


// Middleware
builder.Services.AddScoped<ExceptionHandlingMiddleware>();


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
