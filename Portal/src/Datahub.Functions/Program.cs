using Datahub.Core.Model.Datahub;
using Datahub.Functions;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;

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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Datahub.Infrastructure.ConfigureServices).Assembly));

        services.AddHttpClient(AzureManagementService.ClientName).AddPolicyHandler(
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), 5)));

        services.AddSingleton<AzureConfig>();
        services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
        services.AddSingleton<AzureManagementService>();
        services.AddScoped<ProjectUsageService>();
        services.AddScoped<QueuePongService>();
    })
    .Build();

host.Run();
