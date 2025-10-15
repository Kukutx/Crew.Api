using System.Net.Http.Headers;
using Crew.Application.Abstractions;
using Crew.Application.Auth;
using Crew.Application.Events;
using Crew.Application.Places;
using Crew.Infrastructure.Auth;
using Crew.Infrastructure.Messaging;
using Crew.Infrastructure.Persistence;
using Crew.Infrastructure.Places;
using Crew.Infrastructure.Repositories;
using Crew.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventReadService, EventReadService>();
        services.AddScoped<IFirebaseTokenVerifier, FirebaseTokenVerifier>();

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
            .AddPolicyHandler(GetRetryPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => (int)response.StatusCode == 429)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
}
