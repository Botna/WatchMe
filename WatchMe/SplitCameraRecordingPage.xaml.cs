//using Azure.Storage.Blobs.Models;
//using Azure.Storage.Blobs;
using Camera.MAUI;
using Plugin.Maui.ScreenRecording;
using System.Collections.Concurrent;
using WatchMe.Repository;


namespace WatchMe;

public partial class SplitCameraRecordingPage : ContentPage
{
    private readonly string _videoTimeStampSuffix;
    private readonly IScreenRecording _screenRecorder;
    private readonly ConcurrentBag<string> camerasLoaded = new ConcurrentBag<string>();
    private readonly IVideoRepositoryFactory _videoRepositoryFactory;
    public SplitCameraRecordingPage(IVideoRepositoryFactory videoRepositoryFactory)
    {
        InitializeComponent();
        cameraViewBack.CamerasLoaded += CameraViewBack_CamerasLoaded;
        cameraViewFront.CamerasLoaded += CameraViewFront_CamerasLoaded;
        _videoTimeStampSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

        _videoRepositoryFactory = videoRepositoryFactory;

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

        var videoRepository = _videoRepositoryFactory.GetVideoRepository();

        var backResult = videoRepository.SaveVideoToFileSystem(backVideoBytes, $"Back_{_videoTimeStampSuffix}.mp4");
        var frontResult = videoRepository.SaveVideoToFileSystem(frontVideoBytes, $"Front_{_videoTimeStampSuffix}.mp4");
    }

    //#if ANDROID
    //    private bool SaveToLocalDevice(byte[] videoBytes, string fileName)
    //    {
    //        var context = Platform.CurrentActivity;
    //        var resolver = context.ContentResolver;
    //        var contentValues = new ContentValues();
    //        contentValues.Put(MediaStore.IMediaColumns.DisplayName, fileName);
    //        contentValues.Put(MediaStore.Files.IFileColumns.MimeType, "video/mp4");
    //        contentValues.Put(MediaStore.IMediaColumns.RelativePath, "DCIM/WatchMeVideoCaptures");
    //        try
    //        {
    //            var videoUri = resolver.Insert(MediaStore.Video.Media.ExternalContentUri, contentValues);
    //            var output = resolver.OpenOutputStream(videoUri);
    //            output.Write(videoBytes, 0, videoBytes.Length);
    //            output.Flush();
    //            output.Close();
    //            output.Dispose();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Write(ex.ToString());
    //            return false;
    //        }
    //        contentValues.Put(MediaStore.IMediaColumns.IsPending, 1);
    //        return true;
    //    }
    //#endif



    //Maybe use later for Azure upload.
    //private async Task UploadFileStream(ScreenRecordingFile file)
    //{
    //    var filepath = file.FullPath;
    //    var blobServiceClient = new BlobServiceClient("redacted");

    //    string containerName = "fraudproofcontainer";
    //    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);


    //    BlobClient blobClient = containerClient.GetBlobClient(file.FileName);
    //    var result = await blobClient.UploadAsync(filepath, true);

    //    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    //    {
    //        Console.WriteLine("\t" + blobItem.Name);
    //    }
    //    if (await blobServiceClient.GetBlobContainersAsync() {
    //        //// Create the container and return a container client object
    //        BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

    //        BlobContainerClient blobContainer = new BlobContainerClient(connectionString, containerName);

    //        BlobClient blobClient = blobContainer.GetBlobClient("myTestFile.txt");

    //        var result = await blobClient.UploadAsync(BinaryData.FromString("Hello"), true);
    //    }
}