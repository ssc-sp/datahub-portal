using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;

namespace Datahub.Portal;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args)
            //.ConfigureServices(serviceCollection =>
            //{
            //    serviceCollection.AddSingleton(new ResourceManager("Datahub.Portal.Resources", typeof(Startup).GetTypeInfo().Assembly));
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

                //var appInsightsConfig = hostingContext.Configuration.GetSection("ApplicationInsights");
                //logBuilder.AddApplicationInsights(config => 
                //{ 
                //    config.ConnectionString = appInsightsConfig.GetValue<string>("ConnectionString"); 
                //}, options => {});

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
                    cb.AddUserSecrets<Startup>();
                    if (!ctx.HostingEnvironment.IsDevelopment()) // you'll have to find the right method to check that
                    {
                        StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration);
                    }
                });
            });

}