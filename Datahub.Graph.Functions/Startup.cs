
using System;
using System.Configuration;
using Datahub.Core.EFCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(Datahub.Graph.Functions.Startup))]
namespace Datahub.Graph.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable("datahub-mssql-project");
        if (connectionString is not null)
        {
            builder.Services.AddPooledDbContextFactory<DatahubProjectDBContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContextPool<DatahubProjectDBContext>(options => options.UseSqlServer(connectionString));
        }
    }
}