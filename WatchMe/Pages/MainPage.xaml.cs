using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using WatchMe.Pages;
using WatchMe.Persistance;

#if ANDROID
using Android.App;
using Android.Net.Wifi;
#endif


namespace WatchMe
{
    public partial class MainPage : ContentPage
    {
        public readonly SplitCameraRecordingPage _recordingPage;
        public readonly SettingsPage _settingsPage;
        public readonly ICloudProviderService _cloudProviderService;

        public MainPage(SplitCameraRecordingPage recordingPage, SettingsPage settingsPage, ICloudProviderService cloudProviderService)
        {
            InitializeComponent();
            _recordingPage = recordingPage;
            _settingsPage = settingsPage;
            _cloudProviderService = cloudProviderService;


        }

        private async void OnRecordingPageNav(object sender, EventArgs e)
        {
            var connectionString = await _cloudProviderService.GetAzureConnectionString();

            if (connectionString == null)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                string text = "You do not have your connection string configured.";
                ToastDuration duration = ToastDuration.Long;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);

                await toast.Show(cancellationTokenSource.Token);
                return;
            }

            await Navigation.PushAsync(_recordingPage);
        }

        private async void OnSettingsPageNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(_settingsPage);
        }
    }
}
