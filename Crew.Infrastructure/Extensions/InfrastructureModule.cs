using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Crew.Application.Abstractions;
using Crew.Application.Auth;
using Crew.Application.Events;
using Crew.Application.Moments;
using Crew.Application.Places;
using Crew.Application.Users;
using Crew.Infrastructure.Auth;
using Crew.Infrastructure.Messaging;
using Crew.Infrastructure.Persistence;
using Crew.Infrastructure.Places;
using Crew.Infrastructure.Repositories;
using Crew.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Crew.Infrastructure.Extensions;

public sealed class InfrastructureModule : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? configuration["Database:ConnectionString"]
                ?? throw new InvalidOperationException("Database connection string is not configured.");

            options.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoadTripEventRepository, RoadTripEventRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IEventMetricsRepository, EventMetricsRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUserActivityHistoryRepository, UserActivityHistoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventReadService, EventReadService>();
        services.AddScoped<IGetFeedQueryHandler, GetFeedQueryHandler>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IUserRelationshipService, UserRelationshipService>();
        services.AddScoped<IUserProfileCommandService, UserProfileCommandService>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        services.AddScoped<IMomentService, MomentService>();
        services.AddScoped<IFirebaseTokenVerifier, FirebaseTokenVerifier>();
        services.AddScoped<IFirebaseCustomClaimsService, FirebaseCustomClaimsService>();

        services.AddHostedService<OutboxProcessor>();

        services.AddOptions<GooglePlacesOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ApiKey = config["PLACES_API_KEY"] ?? config[$"{GooglePlacesOptions.SectionName}:ApiKey"];
                options.BaseUrl = config[$"{GooglePlacesOptions.SectionName}:BaseUrl"] ?? options.BaseUrl;
            });

        services.AddHttpClient<IGooglePlacesClient, GooglePlacesClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddResilienceHandler("places-client", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    UseJitter = true, // 随机抖动，避免雪崩
                    ShouldHandle = args =>
                        ValueTask.FromResult(args.Outcome switch
                        {
                            { Exception: HttpRequestException } => true,
                            { Result.StatusCode: var code } when (int)code == 429 => true, // Too Many Requests
                            { Result.StatusCode: var code } when (int)code >= 500 => true, // 服务器错误
                            _ => false
                        })
                });
            });
    }
}
