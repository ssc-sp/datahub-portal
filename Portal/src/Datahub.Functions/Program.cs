using Datahub.Core.Model.Datahub;
using Datahub.Functions;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

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

        services.AddMediatR(typeof(QueueMessageSender<>));
        services.AddHttpClient();
        services.AddSingleton<AzureConfig>();
        services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
        services.AddSingleton<AzureManagementService>();
        services.AddScoped<ProjectUsageService>();
    })
    .Build();

host.Run();
