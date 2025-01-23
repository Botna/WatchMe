using WatchMe.Pages;
using WatchMe.Persistance;
using WatchMe.Services;
using WatchMe.Helpers;
using WatchMe.Config;




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
        public readonly IServiceTest _serviceTest;

        public MainPage(SplitCameraRecordingPage recordingPage, SettingsPage settingsPage, ICloudProviderService cloudProviderService, IServiceTest serviceTest)
        {
            InitializeComponent();
            _recordingPage = recordingPage;
            _settingsPage = settingsPage;
            _cloudProviderService = cloudProviderService;
            _serviceTest = serviceTest;
        }

        private async void OnRecordingPageNav(object sender, EventArgs e)
        {
            var connectionString = await _cloudProviderService.GetAzureConnectionString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                await ToastHelper.CreateToast(WatchMeConstants.Settings_ConnectionStringNotFound_AzureSC);
                return;
            }

            await Navigation.PushAsync(_recordingPage);

            //#if ANDROID
            //            Android.Content.Intent intent = new Android.Content.Intent(Android.App.Application.Context, typeof(ForegroundServiceDemo));
            //            Android.App.Application.Context.StartForegroundService(intent);
            //#endif
            //_serviceTest.StartCameras(null, null);
        }

        private async void OnSettingsPageNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(_settingsPage);
        }
    }
}
