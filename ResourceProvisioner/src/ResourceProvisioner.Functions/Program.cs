using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
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
        services.AddResourceProvisionerApplicationServices(hostContext.Configuration);
        services.AddResourceProvisionerInfrastructureServices(hostContext.Configuration);
        services.AddFeatureManagement();
        
services.AddMassTransitForAzureFunctions(x =>
       {
           x.AddConsumersFromNamespaceContaining<ResourceRunRequest>();
       }, "DatahubServiceBus:ConnectionString");

    })
    .Build();

host.Run();
