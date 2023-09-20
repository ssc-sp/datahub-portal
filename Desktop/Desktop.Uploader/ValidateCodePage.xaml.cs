using Datahub.Core.DataTransfers;

namespace Datahub.Maui.Uploader
{
    public partial class UploadCodePage : ContentPage
    {
        public UploadCodePage()
        {
            InitializeComponent();
        }


        private async void UploadCodeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CredentialEncoder.IsValid(UploadCodeText.Text))
            {
                ValidateCodeButton.IsEnabled = true;
                ValidateCodeButton.Text = "Continue";
                await ContinueFlowWithUploadCode(UploadCodeText.Text);
            } else
            {
                ValidateCodeButton.IsEnabled = false;
                ValidateCodeButton.Text = "Please Enter Valid Code";
            }
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
                Uri uri = new Uri("https://federal-science-datahub.canada.ca/resources/126b3f7a-e320-dc44-3707-e748b998b094");
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
            if (Clipboard.Default.HasText)
            {
                var clipboard = await Clipboard.Default.GetTextAsync();
                if (CredentialEncoder.IsValid(clipboard))
                {
                    await ContinueFlowWithUploadCode(clipboard);
                }
            }
        }

        private static async Task ContinueFlowWithUploadCode(string uploadCode)
        {
            (Application.Current as App).Context.Credentials = CredentialEncoder.DecodeCredentials(uploadCode);
            await Shell.Current.GoToAsync("//SpeedTest");            
        }
    }
}
