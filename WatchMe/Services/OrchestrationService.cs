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
        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
        }

        public async Task InitiateRecordingProcedure(CameraView frontCameraView, CameraView backCameraView, string videoTimeStampSuffix)
        {
            var message = "Andrew just started a WatchMe Routine. Click here to watch along: https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            _notificationService.SendTextToConfiguredContact(message);
            var frontSizes = frontCameraView.Camera.AvailableResolutions;
            var lastFrontSize = frontSizes.Last();

            var backSizes = backCameraView.Camera.AvailableResolutions;
            var lastBackSize = backSizes.Last();

            var frontRecordTask = await frontCameraView.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Front_{videoTimeStampSuffix}.mp4"), lastFrontSize);
            var backRecordTask = await backCameraView.StartRecordingAsync(Path.Combine(FileSystem.Current.CacheDirectory, $"Back_{videoTimeStampSuffix}.mp4"), lastBackSize);
        }

        public Task InitiateRecordingProcedure()
        {
            throw new NotImplementedException();
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
