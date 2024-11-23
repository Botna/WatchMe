using Moq;
using WatchMe.Persistance;
using WatchMe.Repository;
using WatchMe.Services;

namespace WatchMe.UnitTests.Services
{
    public class OrchestrationServiceTests
    {
        [Fact]
        public async void ProcessSavedVideoFile_HappyPath()
        {
            var filename = "video.mp4";
            var path = "some/path";
            var combinedPath = Path.Combine(path, filename);
            byte[]? videoBytes = new byte[10];


            var fileSystemServiceMock = new Mock<IFileSystemService>();
            fileSystemServiceMock.Setup(x => x.GetVideoBytesByFile(combinedPath)).Returns(Task.FromResult(videoBytes));
            fileSystemServiceMock.Setup(x => x.SaveVideoToFileSystem(videoBytes, filename)).Returns(true);
            fileSystemServiceMock.Setup(x => x.GetFileStreamOfFile(combinedPath));
            var cloudProviderServiceMock = new Mock<ICloudProviderService>();
            cloudProviderServiceMock.Setup(x => x.UploadContentToCloud(It.IsAny<FileStream>(), filename));

            var service = new OrchestrationService(cloudProviderServiceMock.Object, fileSystemServiceMock.Object);

            await service.ProcessSavedVideoFile(filename, path);

            cloudProviderServiceMock.VerifyAll();
            fileSystemServiceMock.VerifyAll();
        }
    }
}
