using Datahub.Core.Model.Datahub;
using Datahub.Functions;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;
        
        var connectionString = config["datahub-mssql-project"];
        if (connectionString is not null)
        {
            services.AddPooledDbContextFactory<DatahubProjectDBContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContextPool<DatahubProjectDBContext>(options => options.UseSqlServer(connectionString));
        }

        services.AddHttpClient();
        services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
        services.AddSingleton<AzureManagementService>();
    })
    .Build();

host.Run();
