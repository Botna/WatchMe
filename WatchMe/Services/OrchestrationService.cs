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
        private readonly IVideosRepository _videosRepository;
        private readonly ICameraWrapper _cameraWrapper;

        private string _videoTimeStamp;
        private string _frontVideoFileName;
        private string _backVideoFileName;
        private Timer _videoSplitterTimer;

        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService, IDatabaseInitializer databaseInitializer,
            IVideosRepository videosRepository, ICameraWrapper cameraWrapper)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
            _videosRepository = videosRepository;
            _cameraWrapper = cameraWrapper;

            databaseInitializer.Init();
        }

        public void Initialize(CameraView front, CameraView back)
        {
            _videoTimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
            _cameraWrapper.Initialize(front, back);

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

            await StartRecordingAsync(CameraPosition.Front, _frontVideoFileName);
            await StartRecordingAsync(CameraPosition.Back, _backVideoFileName);

            //var autoEvent = new AutoResetEvent(false);
            //var timerCallback = new TimerCallback(ReceiveTimerTick);
            //_videoSplitterTimer = new Timer(timerCallback, autoEvent, 3000, 3000);
        }

        private async Task StartRecordingAsync(CameraPosition position, string filename)
        {
            var sizes = _cameraWrapper.GetAvailableResolutions(position);
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
            await _cameraWrapper.StartRecordingAsync(position, path, sizeToUse);

            await _videosRepository.InsertVideosAsync(new Videos()
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



        public async Task StopRecordingProcedure()
        {
            //_videoSplitterTimer.Dispose();

            var frontCameraStopTask = _cameraWrapper.StopCameraAsync(CameraPosition.Front);
            var backCameraStopTask = _cameraWrapper.StopCameraAsync(CameraPosition.Back);

            await backCameraStopTask;
            var backVideoBytesTask = _fileSystemService.MoveVideoToGallery(_backVideoFileName);

            await frontCameraStopTask;
            var frontVideoBytesTask = _fileSystemService.MoveVideoToGallery(_frontVideoFileName);

            await Task.WhenAll(frontVideoBytesTask, backVideoBytesTask);

            var frontVideo = await _videosRepository.GetVideosByVideoName(_frontVideoFileName);
            frontVideo.TotalBytes = (await frontVideoBytesTask).Count();
            frontVideo.VideoState = VideoStates.Finished.ToString();

            var backVideo = await _videosRepository.GetVideosByVideoName(_backVideoFileName);
            backVideo.TotalBytes = (await backVideoBytesTask).Count();
            backVideo.VideoState = VideoStates.Finished.ToString();

            var recordsUpdated = await _videosRepository.UpdateVideosAsync(frontVideo, backVideo);

            if (!MauiProgram.ISEMULATED)
            {
                var frontVideoFileStream = _fileSystemService.GetFileStreamOfFile(_frontVideoFileName);
                var frontUploadTask = _cloudProviderService.UploadContentToCloud(frontVideoFileStream, _frontVideoFileName);

                var backVideoFileStream = _fileSystemService.GetFileStreamOfFile(_backVideoFileName);
                var backUploadTask = _cloudProviderService.UploadContentToCloud(backVideoFileStream, _backVideoFileName);

                await Task.WhenAll();
            }
        }
    }
}
