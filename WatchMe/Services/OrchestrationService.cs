using WatchMe.Camera;
using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        void Initialize(CameraView frontCameraView, CameraView backCameraView);
        //Task ProcessSavedVideoFile(string filename, string path);
        Task InitiateRecordingProcedure();
        Task StopRecordingProcedure();
        //Task InitiateRecordingProcedure(CameraView frontCameraView, CameraView backCameraView, string videoTimeStampSuffix);
    }

    public class OrchestrationService : IOrchestrationService
    {
        public readonly ICloudProviderService _cloudProviderService;
        public readonly IFileSystemService _fileSystemService;
        public readonly INotificationService _notificationService;

        private CameraView _frontCameraView;
        private CameraView _backCameraView;
        private string _videoTimeStamp;
        private string _frontVideoFileName;
        private int _frontVideoLastByteWritten;
        private string _backVideoFileName;
        private int _backVideoLastByteWritten;
        private Timer _videoSplitterTimer;
        private int _timerCount = 0;
        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
        }

        public void Initialize(CameraView front, CameraView back)
        {
            _videoTimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
            _frontCameraView = front;
            _backCameraView = back;

            _frontVideoFileName = $"Front_{_videoTimeStamp}";
            _backVideoFileName = $"Back_{_videoTimeStamp}";
        }

        public async Task InitiateRecordingProcedure()
        {
            if (!MauiProgram.ISEMULATED)
            {
                var message = "Andrew just started a WatchMe Routine. Click here to watch along: https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                await _notificationService.SendTextToConfiguredContact(message);
            }

            await StartRecordingAsync(_frontCameraView, _fileSystemService.BuildCacheFileDirectory($"{_frontVideoFileName}.mp4"));
            await StartRecordingAsync(_backCameraView, _fileSystemService.BuildCacheFileDirectory($"{_backVideoFileName}.mp4"));

            //For now, we read in however many bytes are new and available every 3 seconds, and kick it off the phone.
            //TODO - Handle in background process, and figure out a cleaner way to do this without dual upload.
            var autoEvent = new AutoResetEvent(false);
            var timerCallback = new TimerCallback(ReceiveTimerTick);
            _videoSplitterTimer = new Timer(timerCallback, autoEvent, 3000, 3000);
        }

        private Task<CameraResult> StartRecordingAsync(CameraView cameraView, string path)
        {
            var sizes = cameraView.Camera.AvailableResolutions;

            return cameraView.StartRecordingAsync(path, FindSmallestSize(sizes));
        }

        private Size FindSmallestSize(List<Size> sizes)
        {
            return sizes.MinBy(size => size.Width * size.Height);
        }

        private void ReceiveTimerTick(object? objectDetails)
        {

            //NOT THREADSAFE, will fail if video is being written at a certain speed.


            //var frontPicTask = Task.Run(async () =>
            //{
            //    var stream = await _frontCameraView.TakePhotoAsync(Camera.ImageFormat.JPEG);
            //    if (stream != null)
            //    {
            //_fileSystemService.SaveImageStreamToFile(stream, $"Front_{DateTime.Now:yy_MM_dd_HH_mm_ss}_{_vidCount}");
            //    }
            //});

            bool frontFinished = false;
            var currentFrontVideoBytes = _fileSystemService.GetVideoBytesByFile(Path.Combine(FileSystem.Current.CacheDirectory, $"{_frontVideoFileName}.mp4"), _frontVideoLastByteWritten);
            if (currentFrontVideoBytes.Length > 0)
            {
                _frontVideoLastByteWritten += currentFrontVideoBytes.Count();
                _fileSystemService.SaveVideoToFileSystem(currentFrontVideoBytes, $"chunked_{_frontVideoFileName}_{_timerCount}.mp4");

            }
            else
            {
                frontFinished = true;
            }

            var backFinished = false;
            var currentBackVideoBytes = _fileSystemService.GetVideoBytesByFile(Path.Combine(FileSystem.Current.CacheDirectory, $"{_backVideoFileName}.mp4"), _backVideoLastByteWritten);
            if (currentFrontVideoBytes.Length > 0)
            {
                _backVideoLastByteWritten += currentFrontVideoBytes.Count();
                _fileSystemService.SaveVideoToFileSystem(currentFrontVideoBytes, $"chunked_{_backVideoFileName}_{_timerCount}.mp4");

            }
            else
            {
                backFinished = true;
            }


            if (frontFinished && backFinished)
            {
                _videoSplitterTimer.Dispose();
            }
            _timerCount++;
        }

        public async Task ProcessSavedVideoFile(string filename, string path)
        {
            var fullFilePath = Path.Combine(path, filename);
            var totalVideoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);
            if (totalVideoBytes == null)
            {
                throw new Exception("Video file couldn't be opened");
            }


            _fileSystemService.SaveVideoToFileSystem(totalVideoBytes, filename);


            if (!MauiProgram.ISEMULATED)
            {
                var videoFileStream = _fileSystemService.GetFileStreamOfFile(fullFilePath);
                await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
            }

            //var bytesSize = 40960;
            //var currentByte = 0;
            //var count = 0;
            //while (currentByte < totalVideoBytes.Count())
            //{
            //    var videoBytes = _fileSystemService.GetVideoBytesByFile(fullFilePath, currentByte, bytesSize);
            //    currentByte += videoBytes.Count();
            //    _fileSystemService.SaveVideoToFileSystem(videoBytes, $"chunked_{count}.mp4");
            //    count++;
            //}
        }



        public async Task StopRecordingProcedure()
        {
            _videoSplitterTimer.Dispose();

            var frontCameraStopTask = _frontCameraView.StopCameraAsync();
            var backCameraStopTask = _backCameraView.StopCameraAsync();

            await frontCameraStopTask;
            var backVideoProcessingTask = ProcessSavedVideoFile($"{_backVideoFileName}.mp4", FileSystem.Current.CacheDirectory);

            await backCameraStopTask;
            var frontVideoProcessingTask = ProcessSavedVideoFile($"{_frontVideoFileName}.mp4", FileSystem.Current.CacheDirectory);

            await Task.WhenAll(backVideoProcessingTask, frontVideoProcessingTask);
        }
    }
}
