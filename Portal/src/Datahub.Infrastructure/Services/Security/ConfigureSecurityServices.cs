using Datahub.Application.Services.Security;
using Datahub.Shared.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure.Services.Security;

public static class ConfigureSecurityServices
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IKeyVaultUserService, KeyVaultUserService>();
        services.AddSingleton<ISystemTokenCredentialService, SystemTokenCredentialService>();
        services.AddKeyedSingleton<ISystemTokenCredentialService, InfraTokenCredentialService>(SystemTokenCredentialServiceKeys.Infra);
        services.AddScoped<IUserTokenCredentialService, UserTokenCredentialService>();
        services.AddSingleton<IServiceAuthManager, ServiceAuthManager>();
        services.AddSingleton<AzAccessTokenManager>();

        return services;
    }
}
