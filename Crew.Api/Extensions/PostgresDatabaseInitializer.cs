using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Crew.Api.Extensions;

internal static class PostgresDatabaseInitializer
{
    public static void EnsureDatabase(AppDbContext context, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        if (!context.Database.IsNpgsql())
        {
            return;
        }

        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Skipping database creation because no connection string is configured.");
            return;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName) ||
            string.Equals(databaseName, "postgres", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres"
            };

            using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
            connection.Open();

            using var existsCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name;", connection);
            existsCommand.Parameters.AddWithValue("name", databaseName);

            var exists = existsCommand.ExecuteScalar() is not null;
            if (exists)
            {
                return;
            }

            using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\";", connection);
            createCommand.ExecuteNonQuery();

            logger.LogInformation("Created PostgreSQL database {DatabaseName}.", databaseName);
        }
        catch (PostgresException ex) when (string.Equals(ex.SqlState, PostgresErrorCodes.DuplicateDatabase, StringComparison.Ordinal))
        {
            logger.LogDebug(ex, "Database {DatabaseName} already exists.", databaseName);
        }
        catch (PostgresException ex) when (string.Equals(ex.SqlState, PostgresErrorCodes.InsufficientPrivilege, StringComparison.Ordinal))
        {
            logger.LogError(ex, "Insufficient privileges to create database {DatabaseName}.", databaseName);
            throw new InvalidOperationException($"Unable to create database '{databaseName}'. The configured user does not have permission to create databases. Please create the database manually or update the connection string.", ex);
        }
    }
}
