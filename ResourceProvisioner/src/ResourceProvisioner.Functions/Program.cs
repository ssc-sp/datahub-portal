using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResourceProvisioner.Application;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Functions;
using ResourceProvisioner.Infrastructure;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationServices(hostContext.Configuration);
        services.AddInfrastructureServices(hostContext.Configuration);
        
        services.AddMassTransitForAzureFunctions(x =>
        {
            x.AddConsumersFromNamespaceContaining<ResourceRunRequest>();
        }, "DatahubServiceBus:ConnectionString");

    })
    .Build();

host.Run();
