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

        _orchestrationService = orchestrationService;
        orchestrationService.Initialize(cameraViewFront, cameraViewBack);
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

        await _orchestrationService.StartCameraPreviews();
    }

    private async void StartRecordingFromPreview(object sender, EventArgs e)
    {
        await _orchestrationService.InitiateRecordingProcedure();
    }

    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        await _orchestrationService.StopRecordingProcedure();
    }
}