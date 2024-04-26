using System.Text;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
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
            var azureDevOpsConfiguration = configuration.GetSection("InfrastructureRepository:AzureDevOpsConfiguration")
                .Get<AzureDevOpsConfiguration>();
            
            var azureDevOpsClient = new AzureDevOpsClient(azureDevOpsConfiguration!);
            var accessToken = azureDevOpsClient.AccessToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Token}");
        });
        
        services.AddSingleton<ITerraformService, TerraformService>();

        return services;
    }
}