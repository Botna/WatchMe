using WatchMe.Camera;
using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        void Initialize(CameraView frontCameraView, CameraView backCameraView);
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
        }

        public async Task InitiateRecordingProcedure()
        {
            if (_frontCameraView == null || _backCameraView == null)
            {
                //This is our background process and this wont work yet.
                return;
            }

            _frontVideoFileName = $"Front_{_videoTimeStamp}.mp4";
            _backVideoFileName = $"Back_{_videoTimeStamp}.mp4";

            await _notificationService.SendTextToConfiguredContact();

            await StartRecordingAsync(_frontCameraView, _fileSystemService.BuildCacheFileDirectory(_frontVideoFileName));
            await StartRecordingAsync(_backCameraView, _fileSystemService.BuildCacheFileDirectory(_backVideoFileName));
        }

        public async Task StopRecordingProcedure()
        {
            var frontCameraStopTask = _frontCameraView.StopCameraAsync();
            var backCameraStopTask = _backCameraView.StopCameraAsync();

            await frontCameraStopTask;
            var backVideoProcessingTask = ProcessSavedVideoFile(_backVideoFileName, FileSystem.Current.CacheDirectory);

            await backCameraStopTask;
            var frontVideoProcessingTask = ProcessSavedVideoFile(_frontVideoFileName, FileSystem.Current.CacheDirectory);

            await Task.WhenAll(backVideoProcessingTask, frontVideoProcessingTask);
        }

        public virtual Task<CameraResult> StartRecordingAsync(CameraView cameraView, string path)
        {
            var sizes = cameraView.Camera.AvailableResolutions;

            return cameraView.StartRecordingAsync(path, sizes.Last());
        }

        public async Task ProcessSavedVideoFile(string filename, string path)
        {
            var fullFilePath = Path.Combine(path, filename);
            var videoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);
            if (videoBytes == null)
            {
                throw new Exception("Video file couldn't be opened");
            }
            _fileSystemService.SaveVideoToFileSystem(videoBytes, filename);

            var videoFileStream = _fileSystemService.GetFileStreamOfFile(fullFilePath);

            if (!MauiProgram.ISEMULATED)
            {
                await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
            }
        }
    }
}
