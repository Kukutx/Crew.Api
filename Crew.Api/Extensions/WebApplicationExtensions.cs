using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Configuration;
using Crew.Api.Data.DbContexts;
using Crew.Api.Services;
using Crew.Api.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crew.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
        var firebaseAdminService = scope.ServiceProvider.GetRequiredService<IFirebaseAdminService>();
        var firebaseOptions = scope.ServiceProvider.GetRequiredService<IOptions<FirebaseOptions>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedDataService>>();
        await SeedDataService.SeedDatabaseAsync(
            context,
            firebaseAdminService,
            firebaseOptions,
            logger,
            cancellationToken);
    }

    public static void UseAppSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var googleClientId = app.Configuration[$"{FirebaseOptions.SectionName}:ClientId"];

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
}
