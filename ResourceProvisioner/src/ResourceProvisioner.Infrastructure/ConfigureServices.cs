using Datahub.Application.Services.Security;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using System.Text;

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
            var tokenProvider = new InfraSystemTokenCredentialService(azureDevOpsConfiguration!, new NullLogger<InfraSystemTokenCredentialService>());
            var azureDevOpsClient = new AzureDevOpsClient(azureDevOpsConfiguration!, tokenProvider);
            var accessToken = azureDevOpsClient.AccessToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Token}");
        });
        
        services.AddSingleton<ITerraformService, TerraformService>();

        return services;
    }
}
