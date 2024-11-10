namespace WatchMe
{
    public partial class MainPage : ContentPage
    {
        public readonly SplitCameraRecordingPage _recordingPage;

        public MainPage(SplitCameraRecordingPage recordingPage)
        {
            InitializeComponent();
            _recordingPage = recordingPage;
        }

        private async void OnWatchMeInitiate(object sender, EventArgs e)
        {
            await Navigation.PushAsync(_recordingPage);
        }
    }
}
