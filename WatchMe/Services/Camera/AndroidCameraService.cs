#if ANDROID
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.Views;
using Camera.MAUI;
using Java.Lang;
using Java.Util.Concurrent;

namespace WatchMe.Services.Camera
{
    public class AndroidCameraService : ICameraService
    {
        public MediaRecorder mediaRecorder;
        public CameraDevice cameraDevice;
        private CaptureRequest.Builder previewBuilder;
        private PreviewCaptureStateCallback sessionCallback;
        private IExecutorService executorService;
        public void TryStartRecording(string filename)
        {
            //init
            var context = MauiApplication.Context;
            var cameraManager = (CameraManager)context.GetSystemService("camera");
            var cameralist = new List<CameraInfo>();
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
                    var size = (Android.Util.SizeF)chars.Get(CameraCharacteristics.SensorInfoPhysicalSize);
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


            var fileName = System.IO.Path.Combine(FileSystem.Current.CacheDirectory, filename);
            //start recording
            var camChars = cameraManager.GetCameraCharacteristics(cameralist[0].DeviceId);
            StreamConfigurationMap map = (StreamConfigurationMap)camChars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);


            //this is background task, just do max??
            //var videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
            mediaRecorder = null;

            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                mediaRecorder = new MediaRecorder(context);
            else
                mediaRecorder = new MediaRecorder();
            var resolution = cameralist[1].AvailableResolutions.First();
            //mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetVideoSource(VideoSource.Surface);
            mediaRecorder.SetOutputFormat(OutputFormat.Default);
            mediaRecorder.SetOutputFile(fileName);
            mediaRecorder.SetVideoEncodingBitRate(10000000);
            mediaRecorder.SetVideoFrameRate(30);
            mediaRecorder.SetVideoSize((int)resolution.Width, (int)resolution.Height);
            mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            //mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);

            mediaRecorder.Prepare();


            executorService = Executors.NewSingleThreadExecutor();
            var stateListener = new MyCameraStateCallback(this);
            if (OperatingSystem.IsAndroidVersionAtLeast(28))
                cameraManager.OpenCamera(cameralist[1].DeviceId, executorService, stateListener);
            else
                cameraManager.OpenCamera(cameralist[1].DeviceId, stateListener, null);
            mediaRecorder.Start();
        }

        public void TryStopRecording()
        {
            mediaRecorder?.Stop();
            mediaRecorder?.Dispose();


        }

        public void StartCapture()
        {
            SurfaceTexture texture = new SurfaceTexture(10);
            texture.SetDefaultBufferSize(3264, 2448);
            previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Record);

            var surfaces = new List<OutputConfiguration>();
            var previewSurface = new Surface(texture);
            surfaces.Add(new OutputConfiguration(previewSurface));
            if (mediaRecorder != null)
            {
                surfaces.Add(new OutputConfiguration(mediaRecorder.Surface));
                //surfaces26.Add(mediaRecorder.Surface);
                previewBuilder.AddTarget(mediaRecorder.Surface);
            }
            sessionCallback = new PreviewCaptureStateCallback(this);
            if (OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                SessionConfiguration config = new((int)SessionType.Regular, surfaces, executorService, sessionCallback);
                cameraDevice.CreateCaptureSession(config);
            }
        }

        public void UpdatePreview()
        {
            mediaRecorder?.Start();
        }

        internal void SetZoomFactor(float zoom)
        {
            //if (previewSession != null && previewBuilder != null && cameraView.Camera != null)
            //{
            //    //if (OperatingSystem.IsAndroidVersionAtLeast(30))
            //    //{
            //    //previewBuilder.Set(CaptureRequest.ControlZoomRatio, Math.Max(Camera.MinZoomFactor, Math.Min(zoom, Camera.MaxZoomFactor)));
            //    //}
            //    var destZoom = System.Math.Clamp(zoom, 1, System.Math.Min(6, cameraView.Camera.MaxZoomFactor)) - 1;
            //    Android.Graphics.Rect m = (Rect)camChars.Get(CameraCharacteristics.SensorInfoActiveArraySize);
            //    int minW = (int)(m.Width() / (cameraView.Camera.MaxZoomFactor));
            //    int minH = (int)(m.Height() / (cameraView.Camera.MaxZoomFactor));
            //    int newWidth = (int)(m.Width() - (minW * destZoom));
            //    int newHeight = (int)(m.Height() - (minH * destZoom));
            //    Rect zoomArea = new((m.Width() - newWidth) / 2, (m.Height() - newHeight) / 2, newWidth, newHeight);
            //    previewBuilder.Set(CaptureRequest.ScalerCropRegion, zoomArea);
            //    previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
            //}
        }

    }


    public class MyCameraStateCallback : CameraDevice.StateCallback
    {
        private readonly AndroidCameraService _cameraService;
        public MyCameraStateCallback(AndroidCameraService cameraService)
        {
            _cameraService = cameraService;
        }
        public override void OnOpened(CameraDevice camera)
        {
            if (camera != null)
            {
                _cameraService.cameraDevice = camera;
                _cameraService.StartCapture();
            }
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

    public class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
    {
        private readonly AndroidCameraService _cameraService;
        public PreviewCaptureStateCallback(AndroidCameraService cameraService)
        {
            _cameraService = cameraService;
        }
        public override void OnConfigured(CameraCaptureSession session)
        {
            //cameraView.previewSession = session;
            _cameraService.UpdatePreview();

        }
        public override void OnConfigureFailed(CameraCaptureSession session)
        {
        }
    }

    public class CameraInfo
    {
        public string Name { get; set; }

        public string DeviceId { get; set; }

        public CameraPosition Position { get; set; }

        public bool HasFlashUnit { get; set; }

        public float MinZoomFactor { get; set; }

        public float MaxZoomFactor { get; set; }
        public float HorizontalViewAngle { get; set; }

        public float VerticalViewAngle { get; set; }

        public List<Size> AvailableResolutions { get; internal set; }

        public override string ToString()
        {
            return Name;
        }
    }

}
#endif
