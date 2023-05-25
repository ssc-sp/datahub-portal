namespace Datahub.Desktop.Uploader
{
    public partial class UploadCodePage : ContentPage
    {
        public UploadCodePage()
        {
            InitializeComponent();
        }


        private void UploadCodeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCodeButton.IsEnabled = true;
        }

        private async void ValidateCodeButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Upload");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }

        private async void GetUploadCodeBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Uri uri = new Uri("https://federal-science-datahub.canada.ca/w/DW1/filelist");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            Clipboard.Default.ClipboardContentChanged += Clipboard_ClipboardContentChanged;
        }

        private async void Clipboard_ClipboardContentChanged(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SpeedTest");
        }
    }
}