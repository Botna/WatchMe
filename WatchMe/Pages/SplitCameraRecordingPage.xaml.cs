using Plugin.Maui.ScreenRecording;
using System.Collections.Concurrent;
using WatchMe.Camera;
using WatchMe.Services;


namespace WatchMe;

public partial class SplitCameraRecordingPage : ContentPage
{
    private readonly string _videoTimeStampSuffix;
    private readonly IScreenRecording _screenRecorder;
    private readonly ConcurrentBag<string> camerasLoaded = new ConcurrentBag<string>();
    private readonly IOrchestrationService _orchestrationService;

    public SplitCameraRecordingPage(IOrchestrationService orchestrationService)
    {
        InitializeComponent();
        cameraViewBack.CamerasLoaded += CameraViewBack_CamerasLoaded;
        cameraViewFront.CamerasLoaded += CameraViewFront_CamerasLoaded;
        _videoTimeStampSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

        _orchestrationService = orchestrationService;
    }

    public void CameraViewBack_CamerasLoaded(object sender, EventArgs e)
    {
        camerasLoaded.Add("front");

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await StartRecordingWhenBothCamerasLoaded();
        });
    }

    public void CameraViewFront_CamerasLoaded(object sender, EventArgs e)
    {
        camerasLoaded.Add("back");
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await StartRecordingWhenBothCamerasLoaded();
        });
    }

    private async Task StartRecordingWhenBothCamerasLoaded()
    {
        if (camerasLoaded.Count() == 1)
        {
            return;
        }

        var frontCamera = cameraViewFront.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Front);
        var backCamera = cameraViewBack.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Back);

        cameraViewFront.Camera = frontCamera;
        cameraViewBack.Camera = backCamera;

        await _orchestrationService.InitiateRecordingProcedure(cameraViewFront, cameraViewBack, _videoTimeStampSuffix);
    }

    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        var frontFileName = $"Front_{_videoTimeStampSuffix}.mp4";
        var backFileName = $"Back_{_videoTimeStampSuffix}.mp4";

        var result = await cameraViewFront.StopCameraAsync();
        result = await cameraViewBack.StopCameraAsync();

        var backFileTask = _orchestrationService.ProcessSavedVideoFile(frontFileName, FileSystem.Current.CacheDirectory);
        var frontFileTask = _orchestrationService.ProcessSavedVideoFile(backFileName, FileSystem.Current.CacheDirectory);

        await Task.WhenAll(backFileTask, frontFileTask);
    }
}