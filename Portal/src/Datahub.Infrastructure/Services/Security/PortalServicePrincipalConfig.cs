using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Services.Security
{
    public class PortalServicePrincipalConfig(IConfiguration configuration) : IAzureServicePrincipalConfig
    {
        public string SubscriptionId => configuration.GetSection("AzureAd").GetValue<string>("SubscriptionId") ?? throw new InvalidOperationException("SubscriptionId not found");

        public string TenantId => configuration.GetSection("AzureAd").GetValue<string>("TenantId") ?? throw new InvalidOperationException("TenantId not found");

        public string ClientId => configuration.GetSection("AzureAd").GetValue<string>("ClientId") ?? throw new InvalidOperationException("ClientId not found");

        public string ClientSecret => configuration.GetSection("AzureAd").GetValue<string>("ClientSecret") ?? throw new InvalidOperationException("ClientSecret not found");

        //                    _configuration.GetSection("AzureAd").GetValue<string>("TenantId"),
        //_configuration.GetSection("AzureAd").GetValue<string>("ClientId"), 
        //            _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret"), options);

    }
}
