using Microsoft.Extensions.DependencyInjection;

namespace Datahub.CKAN.Service
{
    public static class CKANSeviceCollectionExtensions
    {
        public static void AddCKANService(this IServiceCollection services)
        {
            services.AddScoped<ICKANService, CKANService>();
        }
    }
}
