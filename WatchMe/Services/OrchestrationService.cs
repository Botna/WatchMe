using WatchMe.Camera;
using WatchMe.Persistance.CloudProviders;
using WatchMe.Persistance.Sqlite;
using WatchMe.Persistance.Sqlite.Tables;
using WatchMe.Repository;
using WatchMe.Services.ForegroundServices;

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
        private readonly IForegroundServiceDispatcher _serviceDispatcher;

        private string _videoTimeStamp;
        private string _frontVideoFileName;
        private string _backVideoFileName;

        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService, INotificationService notificationService, IDatabaseInitializer databaseInitializer,
            IVideosRepository videosRepository, ICameraWrapper cameraWrapper, IForegroundServiceDispatcher serviceDispatcher)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
            _notificationService = notificationService;
            _videosRepository = videosRepository;
            _cameraWrapper = cameraWrapper;
            _serviceDispatcher = serviceDispatcher;

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
            if (MauiProgram.ISEMULATED)
            {
                var allVideos = await _videosRepository.GetAllVideosAsync();
                await _videosRepository.DeleteVideosAsync(allVideos.ToArray());
            }

            if (!MauiProgram.ISEMULATED)
            {
                var message = "Andrew just started a WatchMe Routine. Click here to watch along: https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                await _notificationService.SendTextToConfiguredContact(message);

            }

            //await StartRecordingAsync(CameraPosition.Front, _frontVideoFileName);
            await StartRecordingAsync(CameraPosition.Back, _backVideoFileName);

            _serviceDispatcher.StartVUFS();
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

        public async Task StopRecordingProcedure()
        {
            //_videoUploadForegroundService.StopVUFS();
            //await _cameraWrapper.StopCameraAsync(CameraPosition.Front);
            await _cameraWrapper.StopCameraAsync(CameraPosition.Back);

            //await backCameraStopTask;
            var backVideoBytesTask = _fileSystemService.MoveVideoToGallery(_backVideoFileName);

            //await frontCameraStopTask;
            //var frontVideoBytesTask = _fileSystemService.MoveVideoToGallery(_frontVideoFileName);

            await Task.WhenAll(/*frontVideoBytesTask*/ backVideoBytesTask);

            //var frontVideo = await _videosRepository.GetVideosByVideoName(_frontVideoFileName);
            //frontVideo.TotalBytes = (await frontVideoBytesTask).Count();
            //frontVideo.VideoState = VideoStates.Finished.ToString();

            var backVideo = await _videosRepository.GetVideosByVideoName(_backVideoFileName);
            backVideo.TotalBytes = (await backVideoBytesTask).Count();
            backVideo.VideoState = VideoStates.Finished.ToString();

            //await _videosRepository.UpdateTotalBytesOfVideo(frontVideo.Id, frontVideo.TotalBytes);
            await _videosRepository.UpdateTotalBytesOfVideo(backVideo.Id, backVideo.TotalBytes);
            await _videosRepository.UpdateStateOfVideos(VideoStates.Finished,backVideo.Id);


            //var recordsUpdated = await _videosRepository.UpdateVideosAsync(frontVideo, backVideo);

            //if (!MauiProgram.ISEMULATED)
            //{
            //var frontVideoFileStream = _fileSystemService.GetFileStreamOfFile(_frontVideoFileName);
            //var frontUploadTask = _cloudProviderService.UploadContentToCloud(frontVideoFileStream, _frontVideoFileName);

            //var backVideoFileStream = _fileSystemService.GetFileStreamOfFile(_backVideoFileName);
            //var backUploadTask = _cloudProviderService.UploadContentToCloud(backVideoFileStream, _backVideoFileName);

            //await Task.WhenAll();
            //}

            //var allFiles = await _videosRepository.GetAllVideosAsync();
        }
    }
}
