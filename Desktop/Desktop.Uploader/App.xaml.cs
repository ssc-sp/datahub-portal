namespace Datahub.Maui.Uploader
{
	public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Created += Window_Created;
            return window;
        }

        private async void Window_Created(object sender, EventArgs e)
        {
            const int defaultWidth = 600;
            const int defaultHeight = 450;

            var window = (Window)sender;
            window.Width = defaultWidth;
            window.Height = defaultHeight;
            window.X = -defaultWidth;
            window.Y = -defaultHeight;

            await window.Dispatcher.DispatchAsync(() => { });

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
        }
        
    }
}
