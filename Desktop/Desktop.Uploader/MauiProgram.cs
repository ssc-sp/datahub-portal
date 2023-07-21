using Microsoft.Extensions.Logging;
using Datahub.Maui.Uploader;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using System.Text;
using System.Globalization;
using Datahub.Maui.Uploader.Models;
using Microsoft.AspNetCore.Hosting;
using Datahub.Maui.Uploader.IO;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

namespace Datahub.Maui.Uploader
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddJsonLocalization(options =>
            {
                options.CacheDuration = TimeSpan.FromMinutes(15);
                options.ResourcesPath = "i18n";
                options.FileEncoding = Encoding.GetEncoding("ISO-8859-1");
                options.SupportedCultureInfos = new HashSet<CultureInfo>()
                {
                    new CultureInfo("en-CA"),
                    new CultureInfo("fr-CA")
                };
            });
            builder.Services.AddSingleton<DataHubModel>();
            builder.Services.AddSingleton<SpeedTestResults>();
            builder.Services.AddSingleton<FileUtils>();
            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
            RegisterPages(builder);
            builder.Services.AddSingleton<IWebHostEnvironment, WebHostEnvironment>();
            //Routing.RegisterRoute("ValidateCodePage", typeof(ValidateCodePage));
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterPages(MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<ValidateCodePage>();
            builder.Services.AddSingleton<UploadPage>();
            builder.Services.AddSingleton<SpeedTestPage>();            
        }
    }
}
