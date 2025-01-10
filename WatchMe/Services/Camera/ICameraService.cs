namespace WatchMe.Services.Camera
{
    public interface ICameraService
    {
        public void TryStartRecording(string filepath);
        public void TryStopRecording();
    }
}
