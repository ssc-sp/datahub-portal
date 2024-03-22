using Microsoft.Extensions.Logging;
using Datahub.Maui.Uploader.Models;
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
                .RegisterPages()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseVersionTracking();
                });
            builder.Services.AddSingleton<DataHubModel>();
            builder.Services.AddSingleton<SpeedTestResults>();
            builder.Services.AddSingleton<FileUtils>();
            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
            //Routing.RegisterRoute("ValidateCodePage", typeof(ValidateCodePage));
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        public static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<ValidateCodePage>();
            builder.Services.AddSingleton<UploadPage>();
            builder.Services.AddSingleton<SpeedTestPage>();
            return builder;
        }

    }
}
