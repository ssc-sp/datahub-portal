
using System;
using Datahub.Core.Model.Datahub;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(Datahub.Functions.Startup))]
namespace Datahub.Functions;

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