using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Models.Authentication;
using Crew.Api.Utils;
using Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var client = new HttpClient();
var keys = client
    .GetStringAsync(
        "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com")
    .Result;
var originalKeys = new JsonWebKeySet(keys).GetSigningKeys();
var additionalkeys = client
    .GetStringAsync(
        "https://www.googleapis.com/service_accounts/v1/jwk/securetoken@system.gserviceaccount.com")
    .Result;
var morekeys = new JsonWebKeySet(additionalkeys).GetSigningKeys();
var totalkeys = originalKeys.Concat(morekeys);


// Add services to the container.

builder.Services.AddControllers();

// 配置 Entity Framework Core数据库连接 和 Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddDefaultIdentity<ApplicationUser>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddHttpContextAccessor();

// 配置 Firebase 验证
var projectId = builder.Configuration["Firebase:ProjectId"];
// Configure Firebase Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = totalkeys
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
                    // Use the Firebase UID to find or create the user in your Identity system
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();

                    var user = await userManager.FindByNameAsync(firebaseUid);

                    if (user == null)
                    {
                        // Create a new ApplicationUser in your database if the user doesn't exist
                        user = new ApplicationUser
                        {
                            UserName = firebaseUid,
                            Email = firebaseToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                            FirebaseUserId = firebaseUid,
                            Name = firebaseToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                                   $"Planner {firebaseUid}",
                        };
                        await userManager.CreateAsync(user);
                    }
                }
            }
        };
    });


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
// 配置 Swagger 以针对 Firebase 进行授权
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,

        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/v1/auth", UriKind.Relative),
                Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    { "returnSecureToken", new OpenApiBoolean(true) },
                },
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
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
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
