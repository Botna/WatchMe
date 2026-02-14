using System.Collections.Concurrent;

using WatchMe.Services;


namespace WatchMe;

public partial class SplitCameraRecordingPage : ContentPage
{
    private readonly string _videoTimeStampSuffix;
    private readonly ConcurrentBag<string> camerasLoaded = new ConcurrentBag<string>();
    private readonly IOrchestrationService _orchestrationService;

    private List<Size> AvailableResolutions { get; } = new List<Size>();

    public SplitCameraRecordingPage(IOrchestrationService orchestrationService)
    {
        InitializeComponent();
        _videoTimeStampSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

        _orchestrationService = orchestrationService;
        Loaded += OnPageLoaded;
    }


    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        var cameraRequest = await Permissions.RequestAsync<Permissions.Camera>();
        var microphoneRequest = await Permissions.RequestAsync<Permissions.Microphone>();
        if (cameraRequest != PermissionStatus.Granted || microphoneRequest != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Camera permission is required to use this feature.", "OK");
            return;
        }

        //await viewModel.InitializeCameraAsync(BackCameraView);


        var availableCameras = await BackCameraView.GetAvailableCameras(CancellationToken.None);
        BackCameraView.SelectedCamera = availableCameras.FirstOrDefault();

        AvailableResolutions.Clear();

        if (BackCameraView.SelectedCamera?.SupportedResolutions != null)
        {
            foreach (var resolution in BackCameraView.SelectedCamera.SupportedResolutions)
            {
                AvailableResolutions.Add(resolution);
            }
        }


        Loaded -= OnPageLoaded;
    }

    //public void CameraViewBack_CamerasLoaded(object sender, EventArgs e)
    //{
    //    camerasLoaded.Add("front");

    //    MainThread.BeginInvokeOnMainThread(async () =>
    //    {
    //        await StartRecordingWhenBothCamerasLoaded();
    //    });
    //}

    //public void CameraViewFront_CamerasLoaded(object sender, EventArgs e)
    //{
    //    camerasLoaded.Add("back");
    //    MainThread.BeginInvokeOnMainThread(async () =>
    //    {
    //        await StartRecordingWhenBothCamerasLoaded();
    //    });
    //}

    private async Task StartRecordingWhenBothCamerasLoaded()
    {
        if (camerasLoaded.Count() == 1)
        {
            return;
        }

        //var frontCamera = cameraViewFront.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Front);
        //var backCamera = cameraViewBack.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Back);

        //cameraViewFront.Camera = frontCamera;
        //cameraViewBack.Camera = backCamera;

        await _orchestrationService.InitiateRecordingProcedure();
    }

    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        await _orchestrationService.StopRecordingProcedure();



        //var frontFileName = $"Front_{_videoTimeStampSuffix}.mp4";
        //var backFileName = $"Back_{_videoTimeStampSuffix}.mp4";

        //var result = await cameraViewFront.StopCameraAsync();
        ////result = await cameraViewBack.StopCameraAsync();

        //var frontFileTask = _orchestrationService.ProcessSavedVideoFile(frontFileName, FileSystem.Current.CacheDirectory);
        ////var backFileTask = _orchestrationService.ProcessSavedVideoFile(backFileName, FileSystem.Current.CacheDirectory);

        //await Task.WhenAll(/*backFileTask,*/ frontFileTask);
    }

    async void StartCameraRecordingWithCustomStream(object? sender, EventArgs e)
    {
        //using var threeSecondVideoRecordingStream = new FileStream("recording.mp4");
        //await Camera.StartVideoRecording(stream, CancellationToken.None);

        //await Task.Delay(TimeSpan.FromSeconds(3));

        //await Camera.StopVideoRecording(CancellationToken.None);
        //await FileSaver.SaveAsync("recording.mp4", threeSecondVideoRecordingStream);
    }
}