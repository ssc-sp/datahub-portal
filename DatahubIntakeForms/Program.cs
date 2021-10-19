using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datahub.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CultureInfo.CurrentCulture = new CultureInfo("fr");
            //CultureInfo.CurrentUICulture = new CultureInfo("fr");
            CreateHostBuilder(args)
                //.ConfigureServices(serviceCollection =>
                //{
                //    serviceCollection.AddSingleton(new ResourceManager("Datahub.Portal.Resources", typeof(Startup).GetTypeInfo().Assembly));
                //})  
                .Build()
                .Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                  //  logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                   // logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.ConfigureAppConfiguration((ctx, cb) =>
                    {
                        if (!ctx.HostingEnvironment.IsDevelopment()) // you'll have to find the right method to check that
                        {
                            StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration);
                        }
                    });
                });
    }
}
