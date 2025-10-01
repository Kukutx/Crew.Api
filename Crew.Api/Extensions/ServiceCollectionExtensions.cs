using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Configuration;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Security;
using Crew.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Crew.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(FirebaseOptions.SectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{FirebaseOptions.SectionName}' is missing.");
        }

        services.Configure<FirebaseOptions>(section);
        return services;
    }

    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is required.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IFirebaseAdminService, FirebaseAdminService>();
        services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();
        return services;
    }

    public static IServiceCollection AddApplicationSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var firebaseOptions = configuration
            .GetSection(FirebaseOptions.SectionName)
            .Get<FirebaseOptions>() ?? throw new InvalidOperationException("Firebase configuration is required.");

        if (string.IsNullOrWhiteSpace(firebaseOptions.ProjectId))
        {
            throw new InvalidOperationException("Firebase:ProjectId configuration is required.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                options.Authority = $"https://securetoken.google.com/{firebaseOptions.ProjectId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{firebaseOptions.ProjectId}",
                    ValidateAudience = true,
                    ValidAudience = firebaseOptions.ProjectId,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var firebaseToken = context.SecurityToken as JsonWebToken;
                        var firebaseUid = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                        if (string.IsNullOrEmpty(firebaseUid))
                        {
                            return;
                        }

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
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
                policy.Requirements.Add(new AdminRequirement()));
        });

        return services;
    }

    public static IServiceCollection AddApplicationSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
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

        return services;
    }

    public static IServiceCollection AddApplicationCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
