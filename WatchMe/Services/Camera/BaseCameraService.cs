namespace WatchMe.Services.Camera
{
    public abstract class BaseCameraService : ICameraService
    {
        public abstract void TryStartRecording(string filepath);

        public abstract void TryStopRecording();
    }
}
