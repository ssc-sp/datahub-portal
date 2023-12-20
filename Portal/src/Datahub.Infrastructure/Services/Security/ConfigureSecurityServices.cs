using Datahub.Application.Services.Security;
using Datahub.Core.Services.Achievements;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Security;

public static class ConfigureSecurityServices
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IKeyVaultUserService, KeyVaultUserService>();
        return services;
    }
}