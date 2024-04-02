using Datahub.Core.DataTransfers;
using Datahub.Maui.Uploader.Resources;
using Datahub.Maui.Uploader.Models;

namespace Datahub.Maui.Uploader
{
	public partial class ValidateCodePage : ContentPage
    {

        private readonly DataHubModel model;

        public ValidateCodePage(DataHubModel model)
        {
            InitializeComponent();
            this.model = model;
        }

        private string? ReturnErrorIfNotValid(string code)
        {
            var decoded = CredentialEncoder.DecodeCredentials(code);
            if (decoded != null)
            {
                // check if code is still valid
                if (decoded.SASTokenExpiry > DateTimeOffset.Now)
                {
                    return AppResources.ExpiredCode;
                }
                return null;
            }
            else
                return AppResources.EnterValidCode;
        }

        private async void UploadCodeText_TextChanged(object sender, TextChangedEventArgs e)
        {

            var errorCode = ReturnErrorIfNotValid(UploadCodeText.Text);
            if (errorCode is null)
            {
                ValidateCodeButton.IsEnabled = true;
                ValidateCodeButton.Text = AppResources.Continue;
                await ContinueFlowWithUploadCode(UploadCodeText.Text);
            }
            else
            {
                ValidateCodeButton.IsEnabled = false;
                ValidateCodeButton.Text = errorCode;
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
            try
            {
                if (Clipboard.Default.HasText)
                {
                    var content = await Clipboard.Default.GetTextAsync();
                    await ContinueFlowWithUploadCode(content);
                    //Uri uri = new Uri($"https://federal-science-datahub.canada.ca/w/{model.Credentials.WorkspaceCode}/filelist");
                    //await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }
        }

        private async Task ContinueFlowWithUploadCode(string uploadCode)
        {
            if (!string.IsNullOrEmpty(uploadCode))
            {
                var decoded = CredentialEncoder.DecodeCredentials(uploadCode);
                if (decoded != null && decoded.SASTokenExpiry > DateTimeOffset.Now)
                {
                    await Clipboard.Default.SetTextAsync(null);
                    model.Credentials = decoded;
                    await Shell.Current.GoToAsync("//SpeedTest");
                }
            }
        }
    }
}
