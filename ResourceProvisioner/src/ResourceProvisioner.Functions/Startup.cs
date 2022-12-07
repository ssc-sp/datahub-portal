using ResourceProvisioner.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ResourceProvisioner.Application;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

[assembly: FunctionsStartup(typeof(ResourceProvisioner.Functions.Startup))]

namespace ResourceProvisioner.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.GetContext().Configuration);
        builder.Services.AddScoped<CreateResourceRunCommandHandler>();
    }
}