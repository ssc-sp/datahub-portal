using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResourceProvisioner.Application;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            .Build();
        
        services.AddApplicationServices();
        services.AddInfrastructureServices(config);
        services.AddScoped<CreateResourceRunCommandHandler>();

    })
    .Build();

host.Run();
