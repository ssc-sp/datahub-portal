using Datahub.Application.Services.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure.Services.Security;

public static class ConfigureSecurityServices
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IKeyVaultUserService, KeyVaultUserService>();
        return services;
    }
}