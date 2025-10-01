using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Crew.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Crew.Api.Data.DbContexts;
using Crew.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
        SeedDataService.SeedDatabase(context);
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
