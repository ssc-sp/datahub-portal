using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace Datahub.Core.Data
{

    public enum DbDriver
    {
        SqlServer, Sqlite
    }
    public static class EFTools
    {        

        public static void InitializeDatabase<T>(ILogger logger, IConfiguration configuration, IServiceProvider serviceProvider, bool offline, bool migrate = true, bool ensureDeleteinOffline = true) where T : DbContext
        {
            var factory = serviceProvider.GetService(typeof(IDbContextFactory<T>)) as IDbContextFactory<T>;
            InitializeDatabase<T>(logger, configuration, factory, offline, migrate, ensureDeleteinOffline);
        }

        public static void InitializeDatabase<T>(ILogger logger, IConfiguration configuration, IDbContextFactory<T> factory, bool resetDB, bool migrate = true, bool ensureDeleteinOffline = true) where T : DbContext
        {
            //bool offline, ILogger logger, IDbContextFactory<T> factory, 
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
                        context.Database.Migrate();
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

        public static DbDriver GetDriver(this IConfiguration configuration) => (configuration.GetValue(typeof(string), "DbDriver", "SqlServer").ToString().ToLowerInvariant()) switch
        {
            "sqlite" => DbDriver.Sqlite,
            _ => DbDriver.SqlServer
        };

        public static void ConfigureDbContext<T>(this IServiceCollection services, IConfiguration configuration, string connectionStringName, DbDriver dbDriver) where T : DbContext
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrWhiteSpace(connectionStringName) || string.IsNullOrWhiteSpace(connectionString)) throw new InvalidProgramException($"Cannot configure {typeof(T).Name} - no connection string for '{connectionStringName}':{connectionString}");
            switch (dbDriver)
            {
                case DbDriver.SqlServer:
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


        private static string GetInfo(DatabaseFacade db)
        {
            if (db.IsCosmos()) return db.GetCosmosClient()?.ToString() ?? "CosmosDB - no client";
            if (db.IsRelational()) return $"{db.GetDbConnection().Database}";
            return "NA";
        }

        private static void CreateAndSeedDB<T>(ILogger logger, T context, IConfiguration configuration) where T : DbContext
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
                    logger.LogCritical(ex, "Method doesn't exist");
                }

                //var seedable = context as ISeedable<T>;
                //if (seedable != null)
                //{
                //    //seedable.Seed(context, Configuration);
                //    context.SaveChanges();
                //}
            }
        }

    }
}
