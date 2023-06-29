using Foundation;
using Datahub.Maui.Uploader;

namespace Datahub.Maui.Uploader.Platforms.MacCatalyst
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
