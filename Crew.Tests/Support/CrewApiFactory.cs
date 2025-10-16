using System.Linq;
using Crew.Api;
using Crew.Application.Auth;
using Crew.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crew.Tests.Support;

public sealed class CrewApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestToken = "integration-token";
    public const string TestFirebaseUid = "integration-user";

    private readonly SqliteConnection _connection;
    private Respawner? _respawner;

    public CrewApiFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        _connection.Open();
    }

    public FakeFirebaseTokenVerifier TokenVerifier => Services.GetRequiredService<FakeFirebaseTokenVerifier>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection, sqlite => sqlite.UseNetTopologySuite());
                options.UseSnakeCaseNamingConvention();
            });

            var hostedServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(Crew.Infrastructure.Messaging.OutboxProcessor));
            if (hostedServiceDescriptor is not null)
            {
                services.Remove(hostedServiceDescriptor);
            }

            var tokenVerifierDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IFirebaseTokenVerifier));
            if (tokenVerifierDescriptor is not null)
            {
                services.Remove(tokenVerifierDescriptor);
            }

            services.AddSingleton<FakeFirebaseTokenVerifier>();
            services.AddSingleton<IFirebaseTokenVerifier>(sp => sp.GetRequiredService<FakeFirebaseTokenVerifier>());
        });
    }

    public HttpClient CreateAuthenticatedClient(string token = TestToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("The database has not been initialized yet.");
        }

        await _respawner.ResetAsync(_connection);
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Sqlite,
            SchemasToInclude = new[] { "main" }
        });

        TokenVerifier.SetToken(TestToken, new FirebaseTokenResult(TestFirebaseUid, "Integration Tester", "tester@example.com"));
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
