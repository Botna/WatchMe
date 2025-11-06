using WatchMe.Camera;
using WatchMe.Persistance;
using WatchMe.Persistance.Sqlite;
using WatchMe.Persistance.Sqlite.Tables;
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
        private readonly VideosRepository _videosRepository;

        private CameraView _frontCameraView;
        private CameraView _backCameraView;
        private string _videoTimeStamp;
        private string _frontVideoFileName;
        private string _backVideoFileName;
        private Timer _videoSplitterTimer;

        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService, IDatabaseInitializer databaseInitializer,
            VideosRepository videosRepository)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
            _videosRepository = videosRepository;

            databaseInitializer.Init();
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

            var allItems = await _videosRepository.GetItemsAsync();
            await StartRecordingAsync(_frontCameraView, _frontVideoFileName);
            await StartRecordingAsync(_backCameraView, _backVideoFileName);

            //var autoEvent = new AutoResetEvent(false);
            //var timerCallback = new TimerCallback(ReceiveTimerTick);
            //_videoSplitterTimer = new Timer(timerCallback, autoEvent, 3000, 3000);
        }

        private async Task StartRecordingAsync(CameraView cameraView, string filename)
        {
            var sizes = cameraView.Camera.AvailableResolutions;
            Size sizeToUse;

            if (MauiProgram.ISEMULATED)
            {
                sizeToUse = FindSmallestSize(sizes);
            }
            else
            {
                sizeToUse = sizes.First(x => x.Width == 1920 && x.Height == 1080);
            }

            var path = _fileSystemService.BuildCacheFileDirectory(filename);
            await cameraView.StartRecordingAsync(path, sizeToUse);

            await _videosRepository.InsertItemsAsync(new Videos()
            {
                VideoName = filename,
                VideoState = VideoStates.Recording.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        private Size FindSmallestSize(List<Size> sizes)
        {
            return sizes.MinBy(size => size.Width * size.Height);
        }

        private void ReceiveTimerTick(object? objectDetails)
        {

            //NOT THREADSAFE, will fail if video is being written at a certain speed.

            //bool frontFinished = false;
            //var currentFrontVideoBytes = _fileSystemService.GetVideoBytesByFile(Path.Combine(FileSystem.Current.CacheDirectory, $"{_frontVideoFileName}"), _frontVideoLastByteWritten);
            //if (currentFrontVideoBytes.Length > 0)
            //{
            //    _frontVideoLastByteWritten += currentFrontVideoBytes.Count();
            //    _fileSystemService.SaveVideoToFileSystem(currentFrontVideoBytes, $"chunked_{_frontVideoFileName}_{_timerCount}");

            //}
            //else
            //{
            //    frontFinished = true;
            //}

            //var backFinished = false;
            //var currentBackVideoBytes = _fileSystemService.GetVideoBytesByFile(Path.Combine(FileSystem.Current.CacheDirectory, $"{_backVideoFileName}"), _backVideoLastByteWritten);
            //if (currentFrontVideoBytes.Length > 0)
            //{
            //    _backVideoLastByteWritten += currentFrontVideoBytes.Count();
            //    _fileSystemService.SaveVideoToFileSystem(currentFrontVideoBytes, $"chunked_{_backVideoFileName}_{_timerCount}");

            //}
            //else
            //{
            //    backFinished = true;
            //}


            //if (frontFinished && backFinished)
            //{
            //    _videoSplitterTimer.Dispose();
            //}
            //_timerCount++;
        }

        public async Task<int> ProcessSavedVideoFile(string filename, string path)
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

            return totalVideoBytes?.Length ?? 0;
        }

        public async Task StopRecordingProcedure()
        {
            _videoSplitterTimer.Dispose();

            var frontCameraStopTask = _frontCameraView.StopCameraAsync();
            var backCameraStopTask = _backCameraView.StopCameraAsync();

            await frontCameraStopTask;
            var totalFrontVideoBytesTask = ProcessSavedVideoFile($"{_frontVideoFileName}", FileSystem.Current.CacheDirectory);

            await backCameraStopTask;
            var totalBackVideoBytesTask = ProcessSavedVideoFile($"{_backVideoFileName}", FileSystem.Current.CacheDirectory);

            await Task.WhenAll(totalBackVideoBytesTask, totalFrontVideoBytesTask);

            var frontVideo = await _videosRepository.GetVideosByVideoName(_frontVideoFileName);
            frontVideo.TotalBytes = await totalFrontVideoBytesTask;
            frontVideo.VideoState = VideoStates.Finished.ToString();

            var backVideo = await _videosRepository.GetVideosByVideoName(_backVideoFileName);
            backVideo.TotalBytes = await totalBackVideoBytesTask;
            backVideo.VideoState = VideoStates.Finished.ToString();

            var recordsUpdated = await _videosRepository.UpdateItemsAsync(frontVideo, backVideo);


            var allItems = await _videosRepository.GetItemsAsync();
        }
    }
}
