using Camera.MAUI;
using Plugin.Maui.ScreenRecording;
using System.Collections.Concurrent;
using WatchMe.Persistance;
using WatchMe.Repository;


namespace WatchMe;

public partial class SplitCameraRecordingPage : ContentPage
{
    private readonly string _videoTimeStampSuffix;
    private readonly IScreenRecording _screenRecorder;
    private readonly ConcurrentBag<string> camerasLoaded = new ConcurrentBag<string>();
    private readonly IFileSystemServiceFactory _fileSystemServiceFactory;
    private readonly ICloudProviderService _cloudProviderService;

    public SplitCameraRecordingPage(IFileSystemServiceFactory fileSystemServiceFactory, ICloudProviderService cloudProviderService)
    {
        InitializeComponent();
        cameraViewBack.CamerasLoaded += CameraViewBack_CamerasLoaded;
        cameraViewFront.CamerasLoaded += CameraViewFront_CamerasLoaded;
        _videoTimeStampSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

        _fileSystemServiceFactory = fileSystemServiceFactory;
        _cloudProviderService = cloudProviderService;
    }

    private void CameraViewBack_CamerasLoaded(object sender, EventArgs e)
    {
        camerasLoaded.Add("front");

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await StartRecordingWhenBothCamerasLoaded();
        });
    }

    private void CameraViewFront_CamerasLoaded(object sender, EventArgs e)
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

    //TODO, will be kicked off via other processes later.
    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        var result = await cameraViewFront.StopRecordingAsync();
        result = await cameraViewBack.StopRecordingAsync();


        var paths = Directory.GetFiles(FileSystem.Current.CacheDirectory, "*");

        var backVideoBytes = await File.ReadAllBytesAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Back_{_videoTimeStampSuffix}.mp4"));
        var frontVideoBytes = await File.ReadAllBytesAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Front_{_videoTimeStampSuffix}.mp4"));

        var fileSystemService = _fileSystemServiceFactory.GetVideoRepository();

        var backResult = fileSystemService.SaveVideoToFileSystem(backVideoBytes, $"Back_{_videoTimeStampSuffix}.mp4");
        var frontResult = fileSystemService.SaveVideoToFileSystem(frontVideoBytes, $"Front_{_videoTimeStampSuffix}.mp4");

        var backFileStream = fileSystemService.GetFileStreamOfFile(Path.Combine(FileSystem.Current.CacheDirectory, $"Back_{_videoTimeStampSuffix}.mp4"));
        await _cloudProviderService.UploadContentToCloud(backFileStream, $"Back_{_videoTimeStampSuffix}.mp4");

        var frontFileStream = fileSystemService.GetFileStreamOfFile(Path.Combine(FileSystem.Current.CacheDirectory, $"Front_{_videoTimeStampSuffix}.mp4"));
        await _cloudProviderService.UploadContentToCloud(frontFileStream, $"Front_{_videoTimeStampSuffix}.mp4");
    }
}