using WatchMe.Persistance;
using WatchMe.Repository;

namespace WatchMe.Services
{
    public interface IOrchestrationService
    {
        Task ProcessSavedFile(string filename, string path);
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
        public async Task ProcessSavedFile(string filename, string path)
        {
            var fullFilePath = Path.Combine(path, filename);
            var videoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);
            if (videoBytes == null)
            {
                return;
            }
            _fileSystemService.SaveVideoToFileSystem(videoBytes, filename);

            var videoFileStream = _fileSystemService.GetFileStreamOfFile(fullFilePath);
            await _cloudProviderService.UploadContentToCloud(videoFileStream, filename);
        }
    }
}
