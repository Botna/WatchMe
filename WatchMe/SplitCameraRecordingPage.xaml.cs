#if ANDROID
using Android.Telephony;
using Azure.Storage.Blobs.Models;

using Azure.Storage.Blobs;

#endif
using Camera.MAUI;
using Plugin.Maui.ScreenRecording;
using System.Collections.Concurrent;

namespace WatchMe;

public partial class SplitCameraRecordingPage : ContentPage
{
    private readonly string _videoTimeStampSuffix;
    private readonly IScreenRecording _screenRecorder;
    private readonly ConcurrentBag<string> camerasLoaded = new ConcurrentBag<string>();
    public SplitCameraRecordingPage()
    {
        InitializeComponent();
        cameraViewBack.CamerasLoaded += CameraViewBack_CamerasLoaded;
        cameraViewFront.CamerasLoaded += CameraViewFront_CamerasLoaded;

        cameraViewPaneFront.CamerasLoaded += CameraViewPane_CamerasLoaded;
    }

    private void CameraViewPane_CamerasLoaded(object sender, EventArgs e)
    {

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
        if(camerasLoaded.Count() == 1)
        {
            return;
        }

        cameraViewFront.Camera = frontCamera;
        cameraViewBack.Camera = backCamera;

        await cameraViewBack.StartCameraAsync();
        await cameraViewFront.StartCameraAsync();
        var frontSizes = cameraViewFront.Camera.AvailableResolutions;
        var lastFrontSize = frontSizes.Last();

        var backSizes = cameraViewBack.Camera.AvailableResolutions;
        var lastBackSize = backSizes.Last();

        //var frontRecordTask = cameraViewFront.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Front"), lastFrontSize);
        //var backRecordTask = cameraViewBack.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Front"), lastBackSize);


        //var result = await Task.WhenAll(frontRecordTask, backRecordTask);
        //if (_screenRecorder.IsSupported)
        //{
        //    ScreenRecordingOptions screenRecordOptions = new()
        //    {
        //        EnableMicrophone = true,
        //        SaveToGallery = true,
        //        SavePath = Path.Combine(Path.GetTempPath(), $"{_videoTimeStampSuffix}.mp4")
        //    };

        //    _screenRecorder.StartRecording(screenRecordOptions);
        //}

#if ANDROID

        //PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Sms>();
        //if (status != PermissionStatus.Granted)
        //{
        //    status = await Permissions.RequestAsync<Permissions.Sms>();
        //}

        //var smsM = SmsManager.Default;
        //smsM.SendTextMessage("5037029686", null, "Hi Carrie, im recording stuff", null, null);
#endif
    }

    protected override async void OnNavigatingFrom(NavigatingFromEventArgs e)
    {
        //var result = await cameraViewFront.StopRecordingAsync();
        //result = await cameraViewBack.StopRecordingAsync();


        //var paths = Directory.GetFiles(FileSystem.Current.CacheDirectory, "*");

        //var rawVideoBytes = await File.ReadAllBytesAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Back_{_videoTimeStampSuffix}"));
        //var file = await _screenRecorder.StopRecording();
#if ANDROID
        //await UploadFileStream(file);
#endif

    }

    private async Task UploadFileStream(ScreenRecordingFile file)
    {
#if ANDROID

        var filepath = file.FullPath;
        var blobServiceClient = new BlobServiceClient("redacted");

        string containerName = "fraudproofcontainer";
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);


        BlobClient blobClient = containerClient.GetBlobClient(file.FileName);
        var result = await blobClient.UploadAsync(filepath, true);

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            Console.WriteLine("\t" + blobItem.Name);
        }
        //if( await blobServiceClient.GetBlobContainersAsync() {
        //// Create the container and return a container client object
        //BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

        //BlobContainerClient blobContainer = new BlobContainerClient(connectionString, containerName);

        //BlobClient blobClient = blobContainer.GetBlobClient("myTestFile.txt");

        //var result = await blobClient.UploadAsync(BinaryData.FromString("Hello"), true);
#endif

    }
}