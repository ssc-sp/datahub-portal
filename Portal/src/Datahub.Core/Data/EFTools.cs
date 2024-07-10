using Microsoft.AspNetCore.Hosting;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        var factory = serviceProvider.GetService(typeof(IDbContextFactory<T>)) as IDbContextFactory<T>;
        InitializeDatabase<T>(logger, configuration, factory, offline, migrate, ensureDeleteinOffline);
    }

    public static void InitializeDatabase<T>(ILogger logger, IConfiguration configuration, IDbContextFactory<T> factory, bool resetDB, bool migrate = true, bool ensureDeleteinOffline = true)
        where T : DbContext
    {
        using var context = factory.CreateDbContext();
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
                    context.Database.Migrate();
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

    public static DbDriver GetDriver(this IConfiguration configuration) => configuration.GetValue(typeof(string), "DbDriver", "azure").ToString().ToLowerInvariant() switch
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

        switch (dbDriver)
        {
            case DbDriver.Memory:
                services.AddPooledDbContextFactory<T>(options => options.UseSqlServer(connectionString));
                services.AddDbContextPool<T>(options => options.UseSqlServer(connectionString));
                break;
            case DbDriver.SqlServer:
            case DbDriver.SqlLocalDB:
            case DbDriver.Azure:
                services.AddPooledDbContextFactory<T>(options => options.UseSqlServer(connectionString));
                services.AddDbContextPool<T>(options => options.UseSqlServer(connectionString));
                break;
            case DbDriver.Sqlite:
                services.AddPooledDbContextFactory<T>(options => options.UseSqlite(connectionString));
                services.AddDbContextPool<T>(options => options.UseSqlite(connectionString));
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