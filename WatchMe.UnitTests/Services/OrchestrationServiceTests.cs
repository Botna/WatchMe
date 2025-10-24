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
            var cloudProviderServiceMock = new Mock<ICloudProviderService>(MockBehavior.Strict);
            cloudProviderServiceMock.Setup(x => x.UploadContentToCloud(It.IsAny<FileStream>(), filename)).Returns(Task.CompletedTask);

            var service = new OrchestrationService(cloudProviderServiceMock.Object, fileSystemServiceMock.Object, notificationService: null);

            await service.ProcessSavedVideoFile(filename, path);

            cloudProviderServiceMock.VerifyAll();
            fileSystemServiceMock.VerifyAll();
        }

        //[Fact]
        //public async void InitiateRecording_HappyPath()
        //{
        //    var messageRequest = string.Empty;
        //    var notificationServiceMock = new Mock<INotificationService>(MockBehavior.Strict);
        //    notificationServiceMock.Setup(x => x.SendTextToConfiguredContact(It.IsAny<string>())).Callback<string>(r => messageRequest = r).Returns(Task.CompletedTask);
        //    var fileSystemServiceMock = new Mock<IFileSystemService>(MockBehavior.Strict);
        //    fileSystemServiceMock.Setup(x => x.BuildCacheFileDirectory(It.IsAny<string>())).Returns((string input) => { return input; });

        //    var orchestrationServiceMock = new Mock<OrchestrationService>(MockBehavior.Strict, args: [null, fileSystemServiceMock.Object, notificationServiceMock.Object]);
        //    orchestrationServiceMock.Setup(x => x.StartRecordingAsync(It.IsAny<CameraView>(), It.Is<string>(y => y.Contains("Front")))).Returns(Task.FromResult(CameraResult.Success));
        //    orchestrationServiceMock.Setup(x => x.StartRecordingAsync(It.IsAny<CameraView>(), It.Is<string>(y => y.Contains("Back")))).Returns(Task.FromResult(CameraResult.Success));

        //    await orchestrationServiceMock.Object.InitiateRecordingProcedure(null, null, "someTimeStampSuffix");
        //    messageRequest.Should().Contain("just started a WatchMe Routine");

        //    fileSystemServiceMock.VerifyAll();
        //    notificationServiceMock.VerifyAll();
        //    orchestrationServiceMock.Verify(x => x.StartRecordingAsync(It.IsAny<CameraView>(), It.IsAny<string>()), Times.Exactly(2));
        //}
    }
}
