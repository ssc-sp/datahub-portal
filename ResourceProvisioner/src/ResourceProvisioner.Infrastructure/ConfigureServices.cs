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
        var azureDevOpsConfiguration = configuration.GetSection("InfrastructureRepository:AzureDevOpsConfiguration")
            .Get<AzureDevOpsConfiguration>() ?? throw new ArgumentNullException("AzureDevOpsConfiguration section is missing");
        services.AddSingleton<AzureDevOpsConfiguration>(azureDevOpsConfiguration);
        services.AddSingleton<IAzureDevopsConfiguration>(azureDevOpsConfiguration);
        services.AddSingleton<AzAccessTokenManager>();
        services.AddSingleton<ISystemTokenCredentialService, InfraTokenCredentialService>();
        services.AddKeyedSingleton<ISystemTokenCredentialService, InfraTokenCredentialService>(SystemTokenCredentialServiceKeys.Infra);
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddHttpClient("InfrastructureHttpClient", async client =>
        {
            var credentialService = new InfraTokenCredentialService(azureDevOpsConfiguration);            
            var tokenManager = new AzAccessTokenManager(credentialService, credentialService);
            var clientProvider = new AzureDevOpsClient(azureDevOpsConfiguration, tokenManager);
            var accessToken = await tokenManager.AccessDevopsTokenAsync();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Token}");
        });
        services.AddSingleton<AzureDevOpsClient>();

        services.AddSingleton<ITerraformService, TerraformService>();

        return services;
    }
}
