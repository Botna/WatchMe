using System.Diagnostics;
using WatchMe.Camera;
using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        void Initialize(CameraView frontCameraView, CameraView backCameraView);

        Task StartCameraPreviews();
        Task InitiateRecordingProcedure();
        Task StopRecordingProcedure();
    }

    public class OrchestrationService : IOrchestrationService
    {
        private readonly ICloudProviderService _cloudProviderService;
        private readonly IFileSystemService _fileSystemService;
        private readonly INotificationService _notificationService;

        private CameraView _frontCameraView;
        private CameraView _backCameraView;
        private string _videoTimeStamp;
        private string _frontVideoFileName;
        private string _backVideoFileName;
        private Timer _videoSplitterTimer;
        private int _vidCount;
        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
            _vidCount = 1;
        }

        public void Initialize(CameraView front, CameraView back)
        {
            _videoTimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
            _frontCameraView = front;
            _backCameraView = back;
        }

        public async Task StartCameraPreviews()
        {
            if (_frontCameraView == null || _backCameraView == null)
            {
                //This is our background process and this wont work yet.
                return;
            }

            //_frontVideoFileName = $"Front_{_videoTimeStamp}";
            //var currentFrontVideoFileName = $"{_frontVideoFileName}_{_vidCount}";
            //_backVideoFileName = $"Back_{_videoTimeStamp}";
            //var currentBackVideoFileName = $"{_backVideoFileName}_{_vidCount}";

            //await _notificationService.SendTextToConfiguredContact();

            await _frontCameraView.StartCameraAsync(new Size(1280, 720));
            await _backCameraView.StartCameraAsync(new Size(1280, 720));




            //var stream = await _frontCameraView.TakePhotoAsync();
            //if (stream != null)
            //{
            //    var result = ImageSource.FromStream(() => stream);

            //}
            //var frontStartRecordingTask = StartRecordingAsync(_frontCameraView, _fileSystemService.BuildCacheFileDirectory(currentFrontVideoFileName + ".mp4"));
            //var backStartRecordingTask = StartRecordingAsync(_backCameraView, _fileSystemService.BuildCacheFileDirectory(currentBackVideoFileName + ".mp4"));

            //await Task.WhenAll(frontStartRecordingTask, backStartRecordingTask);

            //var autoEvent = new AutoResetEvent(false);
            //var timerCallback = new TimerCallback(ReceiveTimerTick);
            //_videoSplitterTimer = new Timer(timerCallback, autoEvent, 45, 45);

        }

        public async Task InitiateRecordingProcedure()
        {

            await _frontCameraView.SaveSnapShot(Camera.ImageFormat.JPEG, "some/path/file.jpg");

            //var autoEvent = new AutoResetEvent(false);
            //var timerCallback = new TimerCallback(ReceiveTimerTick);
            //_videoSplitterTimer = new Timer(timerCallback, autoEvent, 45, 45);
            //Recording = true;
            //var stopwatch = new Stopwatch();

            //stopwatch.Start();
            //var counter = 1;
            //while (Recording)
            //{
            //    while (stopwatch.ElapsedMilliseconds < 41)
            //    {
            //    }
            //    var frontPicTask = Task.Run(async () =>
            //    {
            //        var stream = await _frontCameraView.TakePhotoAsync(Camera.ImageFormat.JPEG);
            //        if (stream != null)
            //        {
            //            await _fileSystemService.SaveImageStreamToFile(stream, $"Front_{DateTime.Now:yy_MM_dd_HH_mm_ss}_{counter}");

            //        }
            //    });

            //    var BackPicTask = Task.Run(async () =>
            //    {
            //        var stream = await _backCameraView.TakePhotoAsync(Camera.ImageFormat.JPEG);
            //        if (stream != null)
            //        {
            //            await _fileSystemService.SaveImageStreamToFile(stream, $"Back_{DateTime.Now:yy_MM_dd_HH_mm_ss}_{counter}");

            //        }
            //    });
            //    counter++;
            //    stopwatch.Restart();
            //}

        }

        private async void ReceiveTimerTick(object? objectDetails)
        {
            Debug.WriteLine("******************** ReceivedTimerTick ********************");
            Debug.WriteLine($"******************** ElapsedTime:  ********************");



            //var frontPicTask = Task.Run(async () =>
            //{
            //    var stream = await _frontCameraView.TakePhotoAsync(Camera.ImageFormat.JPEG);
            //    if (stream != null)
            //    {
            //_fileSystemService.SaveImageStreamToFile(stream, $"Front_{DateTime.Now:yy_MM_dd_HH_mm_ss}_{_vidCount}");
            //    }
            //});

            //var BackPicTask = Task.Run(async () =>
            //{
            //    var stream = await _backCameraView.TakePhotoAsync(Camera.ImageFormat.JPEG);
            //    if (stream != null)
            //    {
            //        _fileSystemService.SaveImageStreamToFile(stream, $"Back_{DateTime.Now:yy_MM_dd_HH_mm_ss}_{_vidCount}");
            //    }
            //});

            //_vidCount++;
        }

        public async Task StopRecordingProcedure()
        {
            _videoSplitterTimer.Dispose();

            var frontCameraStopTask = _frontCameraView.StopCameraAsync();
            var backCameraStopTask = _backCameraView.StopCameraAsync();

            await frontCameraStopTask;
            var backVideoProcessingTask = ProcessSavedVideoFile($"{_backVideoFileName}_{_vidCount}.mp4", FileSystem.Current.CacheDirectory);

            await backCameraStopTask;
            var frontVideoProcessingTask = ProcessSavedVideoFile($"{_frontVideoFileName}_{_vidCount}.mp4", FileSystem.Current.CacheDirectory);

            await Task.WhenAll(backVideoProcessingTask, frontVideoProcessingTask);
        }
        //private async Task StopCurrentRecordingAndRestart()
        //{
        //    _vidCount++;
        //    var currentFrontVideoFileName = $"{_frontVideoFileName}_{_vidCount}.mp4";
        //    var currentBackVideoFileName = $"{_backVideoFileName}_{_vidCount}.mp4";

        //    var prevFrontVideoFileName = $"{_frontVideoFileName}_{_vidCount - 1}.mp4";
        //    var prevBackVideoFileName = $"{_backVideoFileName}_{_vidCount - 1}.mp4";

        //    //var frontTask = _frontCameraView.StopRecordingAndRestartAsync();
        //    //var backTask = _backCameraView.StopRecordingndRestartAsync()

        //    var processFrontVidTask = ProcessSavedVideoFile(prevFrontVideoFileName, FileSystem.Current.CacheDirectory);
        //    var processBackVidTask = ProcessSavedVideoFile(prevBackVideoFileName, FileSystem.Current.CacheDirectory);


        //    await Task.WhenAll(processFrontVidTask, processBackVidTask);
        //    var frontTask = await _frontCameraView.StopCameraAsync();
        //    var backTask = await _backCameraView.StopCameraAsync();

        //    //await Task.WhenAll(frontTask, backTask);

        //    //_vidCount++;
        //    //var currentFrontVideoFileName = $"{_frontVideoFileName}_{_vidCount}.mp4";
        //    //var currentBackVideoFileName = $"{_backVideoFileName}_{_vidCount}.mp4";

        //    //await frontTask;
        //    var frontStartedTask = StartRecordingAsync(_frontCameraView, _fileSystemService.BuildCacheFileDirectory(currentFrontVideoFileName));
        //    //await backTask;
        //    var backStartedTask = StartRecordingAsync(_backCameraView, _fileSystemService.BuildCacheFileDirectory(currentBackVideoFileName));

        //    await Task.WhenAll(frontStartedTask, backStartedTask);


        //    currentFrontVideoFileName = $"{_frontVideoFileName}_{_vidCount - 1}.mp4";
        //    currentBackVideoFileName = $"{_backVideoFileName}_{_vidCount - 1}.mp4";
        //    //var processFrontVidTask = ProcessSavedVideoFile(currentFrontVideoFileName, FileSystem.Current.CacheDirectory);
        //    //var processBackVidTask = ProcessSavedVideoFile(currentBackVideoFileName, FileSystem.Current.CacheDirectory);
        //}

        public virtual Task<CameraResult> StartRecordingAsync(CameraView cameraView, string path)
        {
            var sizes = cameraView.Camera.AvailableResolutions;

            return cameraView.StartRecordingAsync(path, sizes.Last());
        }

        public async Task ProcessSavedVideoFile(string filename, string path)
        {
            var fullFilePath = Path.Combine(path, filename);

            try
            {
                var videoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);
                _fileSystemService.SaveVideoToFileSystem(videoBytes, filename);

                var videoFileStream = _fileSystemService.GetFileStreamOfFile(fullFilePath);

                if (!MauiProgram.ISEMULATED)
                {
                    await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.Write($"File was unable to be loaded to save to filesystem: {fullFilePath} ex: {ex.Message}");
            }
        }
    }
}
