using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Metadata.Model;
using NRCan.Datahub.Portal.Data.Finance;
using NRCan.Datahub.Portal.EFCore;
using NRCan.Datahub.ProjectForms.Data.PIP;
using NRCan.Datahub.Shared.EFCore;

namespace NRCan.Datahub.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                 //.ConfigureServices(serviceCollection =>
                 //{
                 //    serviceCollection.AddSingleton(new ResourceManager("NRCan.Datahub.Portal.Resources", typeof(Startup).GetTypeInfo().Assembly));
                 //})  
                 .Build();
           
            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logBuilder) =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.AddConsole();
                    
                    // Providing an instrumentation key here is required if you're using
                    // standalone package Microsoft.Extensions.Logging.ApplicationInsights
                    // or if you want to capture logs from early in the application startup
                    // pipeline from Startup.cs or Program.cs itself.
                    var key = hostingContext.Configuration.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");
                    logBuilder.AddApplicationInsights(key);
                    logBuilder.AddAzureWebAppDiagnostics();

                    // event log only works if app is hosted in a Windows enviroment
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        logBuilder.AddEventLog();
                    }

                    // Adding the filter below to ensure logs of all severity from Program.cs
                    // is sent to ApplicationInsights.
                    logBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Trace);

                    // Adding the filter below to ensure logs of all severity from Startup.cs
                    // is sent to ApplicationInsights.
                    logBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Trace);

                    //  logBuilder.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    //  logBuilder.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                    logBuilder.AddFilter("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogLevel.Information);
                    logBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogLevel.Information);
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
