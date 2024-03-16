using System.Text;
using Datahub.Shared.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddHttpClient("InfrastructureHttpClient", client =>
        {
            // client.BaseAddress = new Uri(configuration["InfrastructureRepository:PullRequestUrl"]);

            var token =
                $"{configuration["InfrastructureRepository:Username"]}:{configuration["InfrastructureRepository:Password"]}";
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
        });
        
        services.AddSingleton<ITerraformService, TerraformService>();
        services.AddScoped<AzureServiceBusForwarder>(provider =>
        {
            var storageConnectionString = configuration["DatahubStorageConnectionString"] ?? configuration["DatahubStorageQueue:ConnectionString"];
            return new AzureServiceBusForwarder(storageConnectionString);
        });
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ForwardingConsumer>();
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
                cfg.UseConsumeFilter(typeof(LoggingFilter<>), context);
            });
        });


        return services;
    }
}