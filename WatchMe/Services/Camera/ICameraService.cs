namespace WatchMe.Services.Camera
{
    public interface ICameraService
    {
        public void TryStartRecording(string filename);
        public void TryStopRecording();
    }
}
