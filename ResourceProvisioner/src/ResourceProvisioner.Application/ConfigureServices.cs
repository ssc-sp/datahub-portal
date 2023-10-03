using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceProvisioner.Application.Common.Behaviours;
using ResourceProvisioner.Application.Config;

namespace ResourceProvisioner.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ResourceProvisioner.Application.ConfigureServices).Assembly));


        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
                
        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);
        services.AddSingleton(resourceProvisionerConfiguration);

        return services;
    }
}