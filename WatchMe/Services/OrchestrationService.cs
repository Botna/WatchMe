using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        Task ProcessSavedVideoFile(string filename, string path);
    }

    public class OrchestrationService : IOrchestrationService
    {
        public readonly ICloudProviderService _cloudProviderService;
        public readonly IFileSystemService _fileSystemService;
        public OrchestrationService(ICloudProviderService cloudProviderService, IFileSystemService fileSystemService)
        {
            _cloudProviderService = cloudProviderService;
            _fileSystemService = fileSystemService;
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
            await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
        }
    }
}
