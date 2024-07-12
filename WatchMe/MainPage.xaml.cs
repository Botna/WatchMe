namespace WatchMe
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnWatchMeInitiate(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SplitCameraRecordingPage());
        }
    }
}
