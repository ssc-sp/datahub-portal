using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core
{
    public static class DatahubModuleTools
    {
        public static void AddModule<T>(this IServiceCollection services, IConfiguration configuration) where T:IDatahubModule
        {
            services.AddModule(typeof(T), configuration);

        }

        public static void AddModule(this IServiceCollection services, Type moduleType, IConfiguration configuration) 
        {
            var module = Activator.CreateInstance(moduleType) as IDatahubModule;
            module.ConfigureDatabases(services, configuration);

        }

        public static void ConfigureModule(this IApplicationBuilder builder, Type type)
        {
            var module = (IDatahubModule)Activator.CreateInstance(type);
            var settings = builder.ApplicationServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var context = new DatahubModuleContext(builder.ApplicationServices);
            module.InitializeDatabases(context, settings);
        }

        public static void ConfigureModule<T>(this IApplicationBuilder builder) where T : IDatahubModule
        {
            builder.ConfigureModule(typeof(T));
        }
    }
}
