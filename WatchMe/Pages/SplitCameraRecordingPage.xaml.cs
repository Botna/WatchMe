using Camera.MAUI;
using Plugin.Maui.ScreenRecording;
using System.Collections.Concurrent;
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
        var frontCamera = cameraViewFront.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Front);
        var backCamera = cameraViewBack.Cameras.FirstOrDefault(x => x.Position == CameraPosition.Back);
        if (camerasLoaded.Count() == 1)
        {
            return;
        }

        cameraViewFront.Camera = frontCamera;
        cameraViewBack.Camera = backCamera;

        var frontSizes = cameraViewFront.Camera.AvailableResolutions;
        var lastFrontSize = frontSizes.Last();

        var backSizes = cameraViewBack.Camera.AvailableResolutions;
        var lastBackSize = backSizes.Last();

        var frontRecordTask = await cameraViewFront.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Front_{_videoTimeStampSuffix}.mp4"), lastFrontSize);
        var backRecordTask = await cameraViewBack.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Back_{_videoTimeStampSuffix}.mp4"), lastBackSize);

    }

    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        var frontFileName = $"Front_{_videoTimeStampSuffix}.mp4";
        var backFileName = $"Back_{_videoTimeStampSuffix}.mp4";

        var result = await cameraViewFront.StopRecordingAsync();
        result = await cameraViewBack.StopRecordingAsync();

        var backFileTask = _orchestrationService.ProcessSavedVideoFile(backFileName, FileSystem.Current.CacheDirectory);
        var frontFileTask = _orchestrationService.ProcessSavedVideoFile(frontFileName, FileSystem.Current.CacheDirectory);

        await Task.WhenAll(backFileTask, frontFileTask);
    }
}