#if ANDROID
using Android.Content;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Hardware.Lights;
using Android.Media;
using Camera.MAUI;
using Java.Lang;
using Java.Util.Concurrent;
using static Android.Icu.Text.ListFormatter;





namespace WatchMe.Services.Camera
{
    public class AndroidCameraService : BaseCameraService
    {

        public AndroidCameraService() { }
        public MediaRecorder mediaRecorder;

        public void TryStartCamera()
        {


            //init
            var context = MauiApplication.Context;
            var cameraManager = (CameraManager)context.GetSystemService("camera");
            var cameralist = new List<CameraInfo>();
            //init shit?
            foreach (var id in cameraManager.GetCameraIdList())
            {
                var cameraInfo = new CameraInfo { DeviceId = id, MinZoomFactor = 1 };
                var chars = cameraManager.GetCameraCharacteristics(id);
                if ((int)(chars.Get(CameraCharacteristics.LensFacing) as Java.Lang.Number) == (int)LensFacing.Back)
                {
                    cameraInfo.Name = "Back Camera";
                    cameraInfo.Position = CameraPosition.Back;
                }
                else if ((int)(chars.Get(CameraCharacteristics.LensFacing) as Java.Lang.Number) == (int)LensFacing.Front)
                {
                    cameraInfo.Name = "Front Camera";
                    cameraInfo.Position = CameraPosition.Front;
                }
                else
                {
                    cameraInfo.Name = "Camera " + id;
                    cameraInfo.Position = CameraPosition.Unknow;
                }
                cameraInfo.MaxZoomFactor = (float)(chars.Get(CameraCharacteristics.ScalerAvailableMaxDigitalZoom) as Java.Lang.Number);
                cameraInfo.HasFlashUnit = (bool)(chars.Get(CameraCharacteristics.FlashInfoAvailable) as Java.Lang.Boolean);
                cameraInfo.AvailableResolutions = new();

                try
                {
                    float[] maxFocus = (float[])chars.Get(CameraCharacteristics.LensInfoAvailableFocalLengths);
                    var  size = (Android.Util.SizeF)chars.Get(CameraCharacteristics.SensorInfoPhysicalSize);
                    cameraInfo.HorizontalViewAngle = (float)(2 * System.Math.Atan(size.Width / (maxFocus[0] * 2)));
                    cameraInfo.VerticalViewAngle = (float)(2 * System.Math.Atan(size.Height / (maxFocus[0] * 2)));
                }
                catch { }
                try
                {
                    StreamConfigurationMap otherMap = (StreamConfigurationMap)chars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                    foreach (var s in otherMap.GetOutputSizes(Class.FromType(typeof(ImageReader))))
                        cameraInfo.AvailableResolutions.Add(new(s.Width, s.Height));
                }
                catch
                {
                    if (cameraInfo.Position == CameraPosition.Back)
                        cameraInfo.AvailableResolutions.Add(new(1920, 1080));
                    cameraInfo.AvailableResolutions.Add(new(1280, 720));
                    cameraInfo.AvailableResolutions.Add(new(640, 480));
                    cameraInfo.AvailableResolutions.Add(new(352, 288));
                }
                cameralist.Add(cameraInfo);
            }

            
            var fileName = Path.Combine(FileSystem.Current.CacheDirectory, $"someTestVideo.mp4");
            //start recording
            var camChars = cameraManager.GetCameraCharacteristics(cameralist[0].DeviceId);
            StreamConfigurationMap map = (StreamConfigurationMap)camChars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

            //this is background task, just do max??
            //var videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
            MediaRecorder mediaRecorder = null;

            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                mediaRecorder = new MediaRecorder(context);
            else
                mediaRecorder = new MediaRecorder();
            var resolution = cameralist[0].AvailableResolutions.Last();
            mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetVideoSource(VideoSource.Surface);
            mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            mediaRecorder.SetOutputFile(fileName);
            mediaRecorder.SetVideoEncodingBitRate(10000000);
            mediaRecorder.SetVideoFrameRate(30);
            mediaRecorder.SetVideoSize((int)resolution.Width, (int)resolution.Height);
            mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);

            mediaRecorder.Prepare();
            mediaRecorder.Start();
        }

        public void TryStopCamera()
        {
            mediaRecorder?.Stop();
            mediaRecorder?.Dispose();

            var fileName = Path.Combine(FileSystem.Current.CacheDirectory, $"someTestVideo.mp4");

            var FS = new FileStream(fileName, FileMode.Open);
            //var videoBytes = await _fileSystemService.GetVideoBytesByFile(fullFilePath);


        }


    }




    public class MyCameraStateCallback : CameraDevice.StateCallback
    {
        public MyCameraStateCallback()
        {

        }
        public override void OnOpened(CameraDevice camera)
        {
            //if (camera != null)
            //{
            //    cameraView.cameraDevice = camera;
            //    cameraView.StartPreview();
            //}
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            camera.Close();
            //cameraView.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera?.Close();
            //cameraView.cameraDevice = null;
        }
    }

    public class CameraInfo
    {
        public string Name { get;  set; }

        public string DeviceId { get;  set; }

        public CameraPosition Position { get;  set; }

        public bool HasFlashUnit { get;  set; }

        public float MinZoomFactor { get;  set; }

        public float MaxZoomFactor { get;  set; }
        public float HorizontalViewAngle { get;  set; }

        public float VerticalViewAngle { get;  set; }

        public List<Size> AvailableResolutions { get; internal set; }

        public override string ToString()
        {
            return Name;
        }
    }

}
#endif
