using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceProvisioner.Application.Common.Behaviours;
using ResourceProvisioner.Application.Config;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceProvisioner.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ResourceProvisioner.Application.ConfigureServices).Assembly));


        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));


        services.AddOptions<ResourceProvisionerConfiguration>()
            .Configure(options => configuration.Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        //var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        //configuration.Bind(resourceProvisionerConfiguration);
        //Validator.ValidateObject(resourceProvisionerConfiguration, new ValidationContext(resourceProvisionerConfiguration), validateAllProperties: true);
        //services.AddSingleton(resourceProvisionerConfiguration);

        return services;
    }
}
