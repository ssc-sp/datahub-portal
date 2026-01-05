using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Datahub.Core;

namespace Datahub.Core.Data;

public enum DbDriver
{
    SqlServer, Sqlite, SqlLocalDB, Memory, Azure
}
public static class EFTools
{
    public static void InitializeDatabase<T>(ILogger logger, IConfiguration configuration, IServiceProvider serviceProvider, bool offline, bool migrate = true, bool ensureDeleteinOffline = true)
        where T : DbContext
    {
        var factory = serviceProvider.GetRequiredService<IDbContextFactory<T>>();
        InitializeDatabase<T>(logger, configuration, factory, offline, migrate, ensureDeleteinOffline);
    }

    public static void InitializeDatabase<T>(ILogger logger, IConfiguration configuration, IDbContextFactory<T> factory, bool resetDB, bool migrate = true, bool ensureDeleteinOffline = true)
        where T : DbContext
    {
        using var context = factory.CreateDbContext();
        logger.LogInformation($"Initializing database {GetInfo(context.Database)}-{typeof(T).Name} - reset:{resetDB} - migrate:{migrate}");
        try
        {
            if (resetDB)
            {
                if (ensureDeleteinOffline)
                    context.Database.EnsureDeleted();
                CreateAndSeedDB(logger, context, configuration);
            }
            else
            {
                if (migrate)
                {
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations.");
                    }

                    // Set a longer timeout for migrations (10 minutes)
                    var originalTimeout = context.Database.GetCommandTimeout();
                    try
                    {
                        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
                        logger.LogInformation("Set migration command timeout to 10 minutes");
                        context.Database.Migrate();
                    }
                    finally
                    {
                        // Restore the original timeout
                        context.Database.SetCommandTimeout(originalTimeout);
                        if (originalTimeout.HasValue)
                        {
                            logger.LogInformation("Restored command timeout to {Timeout} seconds", originalTimeout.Value);
                        }
                        else
                        {
                            logger.LogInformation("Restored command timeout to default");
                        }
                    }
                    //TODO:
                    //GetMigrations()
                    //GetAppliedMigrations()
                }
                else
                    CreateAndSeedDB(logger, context, configuration);
            }
            logger.LogInformation($"Successfully initialized database {GetInfo(context.Database)}-{typeof(T).Name}");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"Error initializing database {GetInfo(context.Database)}-{typeof(T).Name}");
        }
    }

    public static string GetConnectionString(this IConfiguration configuration, IWebHostEnvironment environment, string name)
    {
        return configuration.GetConnectionString(name) ?? throw new ArgumentNullException($"ASPNETCORE_CONNECTION STRING ({name}) in Enviroment ({environment.EnvironmentName}).");
    }

    public static DbDriver GetDriver(this IConfiguration configuration) => (configuration.GetValue(typeof(string), "DbDriver", "azure").ToString()!).ToLowerInvariant() switch
    {
        "sqlite" => DbDriver.Sqlite,
        "memory" => DbDriver.Memory,
        "sqlserver" => DbDriver.SqlServer,
        "azure" => DbDriver.Azure,
        "sqllocaldb" => DbDriver.SqlLocalDB,
        _ => DbDriver.Azure
    };

    public static void ConfigureDbContext<T>(this IServiceCollection services, IConfiguration configuration, string connectionStringName, DbDriver dbDriver)
        where T : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionStringName) || string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidProgramException($"Cannot configure {typeof(T).Name} - no connection string for '{connectionStringName}':{connectionString}");
        }

        // use existing helper to detect development environment
        var isDev = DevTools.IsDevelopment();

        // helper to add ConfigureWarnings when in dev
        Action<DbContextOptionsBuilder> BuildSqlServerOptions()
            => options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Set default command timeout to 30 seconds for application operations
                    sqlOptions.CommandTimeout(30);
                });
                if (isDev)
                {
                    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
                }
            };

        Action<DbContextOptionsBuilder> BuildSqliteOptions()
            => options =>
            {
                options.UseSqlite(connectionString, sqliteOptions =>
                {
                    // Set default command timeout to 30 seconds for application operations
                    sqliteOptions.CommandTimeout(30);
                });
                if (isDev)
                {
                    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
                }
            };

        switch (dbDriver)
        {
            case DbDriver.Memory:
                services.AddPooledDbContextFactory<T>(BuildSqlServerOptions());
                services.AddDbContextPool<T>(BuildSqlServerOptions());
                break;
            case DbDriver.SqlServer:
            case DbDriver.SqlLocalDB:
            case DbDriver.Azure:
                services.AddPooledDbContextFactory<T>(BuildSqlServerOptions());
                services.AddDbContextPool<T>(BuildSqlServerOptions());
                break;
            case DbDriver.Sqlite:
                services.AddPooledDbContextFactory<T>(BuildSqliteOptions());
                services.AddDbContextPool<T>(BuildSqliteOptions());
                break;
            default:
                throw new ArgumentException("Invalid DB driver");
        }
    }

    public static void ConfigureDbContext<TGen, Tsql, Tsqlite>(this IServiceCollection services, IConfiguration configuration, string connectionStringName, DbDriver dbDriver)
        where TGen : DbContext
        where Tsql : DbContext
        where Tsqlite : DbContext
    {
        ConfigureDbContext<TGen>(services, configuration, connectionStringName, dbDriver);
        switch (dbDriver)
        {
            case DbDriver.Memory:
            case DbDriver.SqlServer:
            case DbDriver.SqlLocalDB:
            case DbDriver.Azure:
                ConfigureDbContext<Tsql>(services, configuration, connectionStringName, dbDriver);
                break;
            case DbDriver.Sqlite:
                ConfigureDbContext<Tsqlite>(services, configuration, connectionStringName, dbDriver);
                break;
            default:
                throw new ArgumentException("Invalid DB driver");
        }
    }

    private static string GetInfo(DatabaseFacade db)
    {
        if (db.IsRelational()) return $"{db.GetDbConnection().Database}";
        return "NA";
    }

    private static void CreateAndSeedDB<T>(ILogger logger, T context, IConfiguration configuration)
        where T : DbContext
    {
        if (context.Database.EnsureCreated())
        {
            dynamic d = context;
            try
            {
                d.Seed(context, configuration);
                context.SaveChanges();
            }
            catch (RuntimeBinderException ex)
            {
                logger.LogCritical(ex, "Seed(context, configuration) method doesn't exist");
            }
        }
    }
}
