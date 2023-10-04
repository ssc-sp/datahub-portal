using Datahub.Core.DataTransfers;
using Datahub.Maui.Uploader.Resources;

namespace Datahub.Maui.Uploader
{
    public partial class ValidateCodePage : ContentPage
    {

        public ValidateCodePage()
        {
            
            InitializeComponent();
        }


        private async void UploadCodeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CredentialEncoder.IsValid(UploadCodeText.Text))
            {
                ValidateCodeButton.IsEnabled = true;
                ValidateCodeButton.Text = AppResources.Continue;
                await ContinueFlowWithUploadCode(UploadCodeText.Text);
            } else
            {
                ValidateCodeButton.IsEnabled = false;
                ValidateCodeButton.Text = AppResources.EnterValidCode;
            }
        }

        private async void ValidateCodeButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Upload");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

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
