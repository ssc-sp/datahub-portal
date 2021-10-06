using Microsoft.Extensions.DependencyInjection;

namespace NRCan.Datahub.CKAN.Service
{
    public static class CKANSeviceCollectionExtensions
    {
        public static void AddCKANService(this IServiceCollection services)
        {
            services.AddScoped<ICKANService, CKANService>();
        }
    }
}
