using Crew.Infrastructure.Extensions;
using Crew.SeedDataImporter;
using Crew.SeedDataImporter.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var module = new InfrastructureModule();
module.Install(builder.Services, builder.Configuration);

builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection("Seed"));
builder.Services.AddSingleton<SeedDataImporterService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var importer = scope.ServiceProvider.GetRequiredService<SeedDataImporterService>();
var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedDataImporter");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    await importer.RunAsync(cts.Token);
    logger.LogInformation("Seed data import completed successfully.");
    return 0;
}
catch (OperationCanceledException)
{
    logger.LogWarning("Seed data import was cancelled.");
    return 2;
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while importing seed data.");
    return 1;
}
