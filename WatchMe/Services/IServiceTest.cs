using Camera.MAUI;

namespace WatchMe.Services
{
    public interface IServiceTest
    {
        void StartCameras(CameraView front, CameraView back);
        void StopCameras();
    }
}
