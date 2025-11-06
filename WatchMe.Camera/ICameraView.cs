namespace WatchMe.Camera;
public interface ICameraView
{
    public FlashMode FlashMode { get; set; }
    public Task<CameraResult> StartRecordingAsync(string file, Size Resolution = default);
    public Task<CameraResult> StopCameraAsync();
    public CameraInfo Camera { get; set; }
}
