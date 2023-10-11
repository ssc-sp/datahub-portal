using Datahub.Maui.Uploader.Resources;

namespace Datahub.Maui.Uploader
{
    public partial class AppShell : Shell
    {
        public string CurrentVersion { get; set; }
        public string Title { get; set; }

        public AppShell()
        {
            InitializeComponent();
            CurrentVersion = VersionTracking.Default.CurrentVersion.ToString();
            Title = $"{AppResources.DatahubUploader} {CurrentVersion}";
            this.BindingContext = this;
        }
    }
}
