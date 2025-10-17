using System;
using System.IO;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Crew.Infrastructure.Persistence.Factories;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = BuildConfiguration(environment);

        var connectionString = configuration.GetConnectionString("Default")
            ?? configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database connection string is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration(string environment)
    {
        var basePath = ResolveBasePath(Directory.GetCurrentDirectory());

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveBasePath(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            var directAppSettings = Path.Combine(directory.FullName, "appsettings.json");
            if (File.Exists(directAppSettings))
            {
                return directory.FullName;
            }

            var apiDirectory = Path.Combine(directory.FullName, "Crew.Api", "appsettings.json");
            if (File.Exists(apiDirectory))
            {
                return Path.Combine(directory.FullName, "Crew.Api");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate appsettings.json for design-time configuration.");
    }
}
