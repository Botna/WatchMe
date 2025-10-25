using WatchMe.Camera;
using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        Task ProcessSavedVideoFile(string filename, string path);
        Task InitiateRecordingProcedure();
        Task InitiateRecordingProcedure(CameraView frontCameraView, CameraView backCameraView, string videoTimeStampSuffix);
    }

    public class OrchestrationService : IOrchestrationService
    {
        public readonly ICloudProviderService _cloudProviderService;
        public readonly IFileSystemService _fileSystemService;
        public readonly INotificationService _notificationService;
        private Timer _videoSplitterTimer;
        private int lastByteProcessed = 0;
        private int timerCount = 0;
        private string _videoTimeStampSuffix;
        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
        }

        public async Task InitiateRecordingProcedure(CameraView frontCameraView, CameraView backCameraView, string videoTimeStampSuffix)
        {
            _videoTimeStampSuffix = videoTimeStampSuffix;
            //var message = "Andrew just started a WatchMe Routine. Click here to watch along: https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            //await _notificationService.SendTextToConfiguredContact(message);

            await StartRecordingAsync(frontCameraView, _fileSystemService.BuildCacheFileDirectory($"Front_{_videoTimeStampSuffix}.mp4"));
            await StartRecordingAsync(backCameraView, _fileSystemService.BuildCacheFileDirectory($"Back_{_videoTimeStampSuffix}.mp4"));

            var autoEvent = new AutoResetEvent(false);
            var timerCallback = new TimerCallback(ReceiveTimerTick);
            _videoSplitterTimer = new Timer(timerCallback, autoEvent, 3000, 3000);
        }

        //Todo, figure out how to remove public here
        public virtual Task<CameraResult> StartRecordingAsync(CameraView cameraView, string path)
        {
            var sizes = cameraView.Camera.AvailableResolutions;

            return cameraView.StartRecordingAsync(path, sizes.Last());
        }

        public Task InitiateRecordingProcedure()
        {
            throw new NotImplementedException();
        }

        private async void ReceiveTimerTick(object? objectDetails)
        {
            var path = FileSystem.Current.CacheDirectory;
            var baseFileName = $"Front_{_videoTimeStampSuffix}";

            var videoBytes = await _fileSystemService.GetVideoBytesByFile(Path.Combine(path, $"{baseFileName}.mp4"), lastByteProcessed);

            lastByteProcessed += videoBytes.Count();
            _fileSystemService.SaveVideoToFileSystem(videoBytes, $"chunked_{baseFileName}_{timerCount}.mp4");
            timerCount++;
        }

        public async Task ProcessSavedVideoFile(string filename, string path)
        {
            var fullFilePath = Path.Combine(path, filename);
            var videoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);
            if (videoBytes == null)
            {
                throw new Exception("Video file couldn't be opened");
            }
            //_fileSystemService.SaveVideoToFileSystem(videoBytes, filename);

            //var videoFileStream = _fileSystemService.GetFileStreamOfFile(fullFilePath);

            //if (!MauiProgram.ISEMULATED)
            //{
            //    await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
            //}
        }
    }
}
