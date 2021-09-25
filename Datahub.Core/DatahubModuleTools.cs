using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared
{
    public static class DatahubModuleTools
    {
        public static void AddModule<T>(this IServiceCollection services, IConfiguration configuration) where T:IDatahubModule
        {
            var module = (T)Activator.CreateInstance(typeof(T));            
            module.ConfigureDatabases(services, configuration);

        }

        public static void ConfigureModule<T>(this IApplicationBuilder builder) where T : IDatahubModule
        {
            var module = (T) Activator.CreateInstance(typeof(T));
            var settings = builder.ApplicationServices.GetService(typeof(IConfiguration));
            var context = new DatahubModuleContext(builder.ApplicationServices);
            module.InitializeDatabases(context);
        }
    }
}
