using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResourceProvisioner.Application;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Infrastructure;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(hostContext.Configuration);
        services.AddScoped<CreateResourceRunCommandHandler>();

    })
    .Build();

host.Run();
