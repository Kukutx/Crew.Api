using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Utils;
using Crew.Api.Security;
using Crew.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpClient();

// 配置 Google Places API 的 HttpClient
var apiKey = builder.Configuration["Google:Places:ApiKey"] ?? "";
builder.Services.AddHttpClient("GooglePlaces", c =>
{
    c.BaseAddress = new Uri("https://places.googleapis.com/v1/");
    c.DefaultRequestHeaders.Add("X-Goog-Api-Key", apiKey);
    // 统一用 JSON
    c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// 配置 Entity Framework Core数据库连接 和 Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IFirebaseAdminService, FirebaseAdminService>();
builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();

// 配置 Firebase 验证
var projectId = builder.Configuration["Firebase:ProjectId"];
// Configure Firebase Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException("Firebase:ProjectId configuration is required");
        }

        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Receive the JWT token that firebase has provided
                var firebaseToken = context.SecurityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;
                // Get the Firebase UID of this user
                var firebaseUid = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                if (!string.IsNullOrEmpty(firebaseUid))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

                    var user = await dbContext.Users
                        .Include(u => u.Roles)
                            .ThenInclude(r => r.Role)
                        .Include(u => u.Subscriptions)
                        .FirstOrDefaultAsync(u => u.Uid == firebaseUid, context.HttpContext.RequestAborted);

                    if (user is null)
                    {
                        user = new UserAccount
                        {
                            Uid = firebaseUid,
                            Email = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
                            UserName = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
                            DisplayName = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty,
                            AvatarUrl = AvatarDefaults.Normalize(firebaseToken?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value),
                            CreatedAt = DateTime.UtcNow,
                            Status = UserStatuses.Active,
                        };

                        var defaultRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Key == RoleKeys.User, context.HttpContext.RequestAborted);
                        if (defaultRole != null)
                        {
                            user.Roles.Add(new UserRoleAssignment
                            {
                                RoleId = defaultRole.Id,
                                UserUid = user.Uid,
                                GrantedAt = DateTime.UtcNow,
                            });
                        }

                        var freePlan = await dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Key == SubscriptionPlanKeys.Free, context.HttpContext.RequestAborted);
                        if (freePlan != null)
                        {
                            user.Subscriptions.Add(new UserSubscription
                            {
                                PlanId = freePlan.Id,
                                UserUid = user.Uid,
                                AssignedAt = DateTime.UtcNow,
                            });
                        }

                        dbContext.Users.Add(user);
                    }
                    else
                    {
                        var email = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                        var name = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                        var avatar = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            user.Email = email;
                            user.UserName = string.IsNullOrWhiteSpace(user.UserName) ? email : user.UserName;
                        }

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            user.DisplayName = name;
                        }

                        if (!string.IsNullOrWhiteSpace(avatar))
                        {
                            user.AvatarUrl = AvatarDefaults.Normalize(avatar);
                        }

                        user.UpdatedAt = DateTime.UtcNow;
                    }

                    await dbContext.SaveChangesAsync(context.HttpContext.RequestAborted);
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
        policy.Requirements.Add(new AdminRequirement()));
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    const string securityScheme = JwtBearerDefaults.AuthenticationScheme;

    var flows = new OpenApiOAuthFlows
    {
        Password = new OpenApiOAuthFlow
        {
            TokenUrl = new Uri("/v1/auth", UriKind.Relative),
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                { "returnSecureToken", new OpenApiBoolean(true) },
            },
        },
        AuthorizationCode = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
            TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
            Scopes = new Dictionary<string, string>
            {
                { "openid", "Authenticate with your Google account" },
                { "email", "Read your email address" },
                { "profile", "Read your basic profile information" }
            }
        }
    };

    options.AddSecurityDefinition(securityScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Use email/password or Google sign-in (via Firebase Auth) to request an access token and send it as a Bearer token.",
        Flows = flows
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = securityScheme
                }
            },
            new List<string> { "openid", "email", "profile" }
        }
    });
});

// 添加跨域请求支持
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// 调用 SeedDataService
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    SeedDataService.SeedDatabase(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 启用 Swagger & Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var googleClientId = builder.Configuration["Firebase:ClientId"];

        if (!string.IsNullOrWhiteSpace(googleClientId))
        {
            options.OAuthClientId(googleClientId);
        }

        options.OAuthScopeSeparator(" ");
        options.OAuthUsePkce();
        options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
        {
            { "prompt", "select_account" }
        });
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
