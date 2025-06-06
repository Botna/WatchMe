﻿using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util.Concurrent;
using CameraCharacteristics = Android.Hardware.Camera2.CameraCharacteristics;
using Class = Java.Lang.Class;
using Rect = Android.Graphics.Rect;
using RectF = Android.Graphics.RectF;
using Size = Android.Util.Size;
using SizeF = Android.Util.SizeF;

namespace WatchMe.Camera.Platforms.Android;

internal class MauiCameraView : GridLayout
{
    private readonly CameraView cameraView;
    private IExecutorService executorService;
    private bool started = false;
    private int frames = 0;
    private bool initiated = false;
    private bool snapping = false;
    private bool recording = false;
    private readonly Context context;

    private readonly TextureView textureView;
    public CameraCaptureSession previewSession;
    public MediaRecorder mediaRecorder;
    private CaptureRequest.Builder previewBuilder;
    private CameraDevice cameraDevice;
    private readonly MyCameraStateCallback stateListener;
    private Size videoSize;
    private CameraManager cameraManager;
    private AudioManager audioManager;
    private readonly SparseIntArray ORIENTATIONS = new();
    private readonly SparseIntArray ORIENTATIONSFRONT = new();
    private CameraCharacteristics camChars;
    private PreviewCaptureStateCallback sessionCallback;
    private byte[] capturePhoto = null;
    private bool captureDone = false;
    private HandlerThread backgroundThread;
    private Handler backgroundHandler;



    public MauiCameraView(Context context, CameraView cameraView) : base(context)
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" Maui Camera View Constructor - Android - Begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");


        this.context = context;
        this.cameraView = cameraView;

        textureView = new(context);
        stateListener = new MyCameraStateCallback(this);
        AddView(textureView);
        ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
        ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
        ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
        ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
        ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation0, 270);
        ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation90, 0);
        ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation180, 90);
        ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation270, 180);
        InitDevices();
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" Maui Camera View Constructor - Android - End");
        System.Diagnostics.Debug.WriteLine("*****************************************");
    }

    //public MauiCameraView(Context context) : base(context)
    //{
    //    this.context = context;

    //    textureView = new(context);
    //    stateListener = new MyCameraStateCallback(this);
    //    AddView(textureView);
    //    ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
    //    ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
    //    ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
    //    ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
    //    ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation0, 270);
    //    ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation90, 0);
    //    ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation180, 90);
    //    ORIENTATIONSFRONT.Append((int)SurfaceOrientation.Rotation270, 180);
    //    InitDevices();
    //}

    private void InitDevices()
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" Initializing the devices - begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        if (!initiated && cameraView != null)
        {
            cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
            audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
            cameraView.Cameras.Clear();
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
                    SizeF size = (SizeF)chars.Get(CameraCharacteristics.SensorInfoPhysicalSize);
                    cameraInfo.HorizontalViewAngle = (float)(2 * Math.Atan(size.Width / (maxFocus[0] * 2)));
                    cameraInfo.VerticalViewAngle = (float)(2 * Math.Atan(size.Height / (maxFocus[0] * 2)));
                }
                catch { }
                try
                {
                    StreamConfigurationMap map = (StreamConfigurationMap)chars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                    foreach (var s in map.GetOutputSizes(Class.FromType(typeof(ImageReader))))
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
                cameraView.Cameras.Add(cameraInfo);
            }
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                cameraView.Microphones.Clear();
                foreach (var device in audioManager.Microphones)
                {
                    cameraView.Microphones.Add(new MicrophoneInfo { Name = "Microphone " + device.Type.ToString() + " " + device.Address, DeviceId = device.Id.ToString() });
                }
            }
            //Microphone = Micros.FirstOrDefault();
            executorService = Executors.NewSingleThreadExecutor();

            initiated = true;
            cameraView.RefreshDevices();
        }
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" Initializing the devices - end");
        System.Diagnostics.Debug.WriteLine("*****************************************");
    }

    internal async Task<CameraResult> StartRecordingAsync(string file, Microsoft.Maui.Graphics.Size Resolution)
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartRecordingAsync - Begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        var result = CameraResult.Success;
        if (initiated && !recording)
        {
            if (await CameraView.RequestPermissions(true, true))
            {
                if (started) StopCamera();
                if (cameraView.Camera != null)
                {
                    try
                    {
                        camChars = cameraManager.GetCameraCharacteristics(cameraView.Camera.DeviceId);

                        StreamConfigurationMap map = (StreamConfigurationMap)camChars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                        videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
                        recording = true;

                        if (File.Exists(file)) File.Delete(file);

                        if (OperatingSystem.IsAndroidVersionAtLeast(31))
                            mediaRecorder = new MediaRecorder(context);
                        else
                            mediaRecorder = new MediaRecorder();
                        audioManager.Mode = Mode.Normal;
                        mediaRecorder.SetAudioSource(AudioSource.Mic);
                        mediaRecorder.SetVideoSource(VideoSource.Surface);
                        mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
                        mediaRecorder.SetOutputFile(file);
                        mediaRecorder.SetVideoEncodingBitRate(10000000);
                        mediaRecorder.SetVideoFrameRate(30);

                        var maxVideoSize = ChooseMaxVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
                        if (Resolution.Width != 0 && Resolution.Height != 0)
                            maxVideoSize = new((int)Resolution.Width, (int)Resolution.Height);
                        mediaRecorder.SetVideoSize(maxVideoSize.Width, maxVideoSize.Height);

                        mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
                        mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
                        IWindowManager windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                        int rotation = (int)windowManager.DefaultDisplay.Rotation;
                        int orientation = cameraView.Camera.Position == CameraPosition.Back ? orientation = ORIENTATIONS.Get(rotation) : orientation = ORIENTATIONSFRONT.Get(rotation);
                        mediaRecorder.SetOrientationHint(orientation);
                        mediaRecorder.Prepare();

                        if (OperatingSystem.IsAndroidVersionAtLeast(28))
                            cameraManager.OpenCamera(cameraView.Camera.DeviceId, executorService, stateListener);
                        else
                            cameraManager.OpenCamera(cameraView.Camera.DeviceId, stateListener, null);
                        started = true;
                    }
                    catch
                    {
                        result = CameraResult.AccessError;
                    }
                }
                else
                    result = CameraResult.NoCameraSelected;
            }
            else
                result = CameraResult.AccessDenied;
        }
        else
            result = CameraResult.NotInitiated;
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartRecordingAsync - End");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        return result;
    }

    private void StartPreview()
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartPreview - begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        while (textureView.SurfaceTexture == null || !textureView.IsAvailable) Thread.Sleep(100);


        //Show preview
        SurfaceTexture texture = textureView.SurfaceTexture;

        //Dont show
        //SurfaceTexture texture = new SurfaceTexture(10);


        texture.SetDefaultBufferSize(videoSize.Width, videoSize.Height);

        previewBuilder = cameraDevice.CreateCaptureRequest(recording ? CameraTemplate.Record : CameraTemplate.Preview);
        var surfaces = new List<OutputConfiguration>();

        //dont show
        var surfaces26 = new List<Surface>();


        var previewSurface = new Surface(texture);
        surfaces.Add(new OutputConfiguration(previewSurface));

        //Dont show
        surfaces26.Add(previewSurface);


        previewBuilder.AddTarget(previewSurface);
        if (mediaRecorder != null)
        {
            surfaces.Add(new OutputConfiguration(mediaRecorder.Surface));

            //dont show
            surfaces26.Add(mediaRecorder.Surface);

            previewBuilder.AddTarget(mediaRecorder.Surface);
        }

        sessionCallback = new PreviewCaptureStateCallback(this);
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            SessionConfiguration config = new((int)SessionType.Regular, surfaces, executorService, sessionCallback);
            cameraDevice.CreateCaptureSession(config);
        }
        else
        {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            // cameraDevice.CreateCaptureSession(surfaces26, sessionCallback, null);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        }

        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartPreview - end");
        System.Diagnostics.Debug.WriteLine("*****************************************");

    }
    private void UpdatePreview()

    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" UpdatePreview - begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");

        if (null == cameraDevice)
            return;

        try
        {
            //previewBuilder.Set(CaptureRequest.ControlMode, Java.Lang.Integer.ValueOf((int)ControlMode.Auto));
            //Rect m = (Rect)camChars.Get(CameraCharacteristics.SensorInfoActiveArraySize);
            //videoSize = new Size(m.Width(), m.Height());
            AdjustAspectRatio(videoSize.Width, videoSize.Height);
            SetZoomFactor(cameraView.ZoomFactor);
            //previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
            if (recording)
                mediaRecorder?.Start();
        }
        catch (CameraAccessException e)
        {
            e.PrintStackTrace();
        }

        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" UpdatePreview - end");
        System.Diagnostics.Debug.WriteLine("*****************************************");
    }
    internal async Task<CameraResult> StartCameraAsync(Microsoft.Maui.Graphics.Size PhotosResolution)
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartCameraAsync - Begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        var result = CameraResult.Success;
        if (initiated)
        {
            if (await CameraView.RequestPermissions())
            {
                if (started) StopCamera();
                if (cameraView.Camera != null)
                {
                    try
                    {
                        camChars = cameraManager.GetCameraCharacteristics(cameraView.Camera.DeviceId);

                        StreamConfigurationMap map = (StreamConfigurationMap)camChars.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                        videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
                        var maxVideoSize = ChooseMaxVideoSize(map.GetOutputSizes(Class.FromType(typeof(ImageReader))));
                        if (PhotosResolution.Width != 0 && PhotosResolution.Height != 0)
                            maxVideoSize = new((int)PhotosResolution.Width, (int)PhotosResolution.Height);

                        backgroundThread = new HandlerThread("CameraBackground");
                        backgroundThread.Start();
                        backgroundHandler = new Handler(backgroundThread.Looper);

                        if (OperatingSystem.IsAndroidVersionAtLeast(28))
                            cameraManager.OpenCamera(cameraView.Camera.DeviceId, executorService, stateListener);
                        else
                            cameraManager.OpenCamera(cameraView.Camera.DeviceId, stateListener, null);

                        started = true;
                    }
                    catch
                    {
                        result = CameraResult.AccessError;
                    }
                }
                else
                    result = CameraResult.NoCameraSelected;
            }
            else
                result = CameraResult.AccessDenied;
        }
        else
            result = CameraResult.NotInitiated;

        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StartCameraAsync - End");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        return result;
    }
    internal Task<CameraResult> StopRecordingAsync()
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StopRecordingAsync - Begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        recording = false;
        var task = StartCameraAsync(cameraView.PhotosResolution);

        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StopRecordingAsync - End");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        return task;
    }

    internal CameraResult StopCamera()
    {
        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StopCamera - Begin");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        CameraResult result = CameraResult.Success;
        if (initiated)
        {
            try
            {
                mediaRecorder?.Stop();
                mediaRecorder?.Dispose();
            }
            catch { }
            try
            {
                backgroundThread?.QuitSafely();
                backgroundThread?.Join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            catch { }
            try
            {
                previewSession?.StopRepeating();
                previewSession?.AbortCaptures();
                previewSession?.Dispose();
            }
            catch { }
            try
            {
                cameraDevice?.Close();
                cameraDevice?.Dispose();
            }
            catch { }
            previewSession = null;
            cameraDevice = null;
            previewBuilder = null;
            mediaRecorder = null;
            started = false;
            recording = false;
        }
        else
            result = CameraResult.NotInitiated;

        System.Diagnostics.Debug.WriteLine("*****************************************");
        System.Diagnostics.Debug.WriteLine(" StopCamera - End");
        System.Diagnostics.Debug.WriteLine("*****************************************");
        return result;
    }
    internal void DisposeControl()
    {
        try
        {
            if (started) StopCamera();
            executorService?.Shutdown();
            executorService?.Dispose();
            RemoveAllViews();
            textureView?.Dispose();
            Dispose();
        }
        catch { }
    }

    public void UpdateMirroredImage()
    {
        if (cameraView != null && textureView != null)
        {
            if (cameraView.MirroredImage)
                textureView.ScaleX = -1;
            else
                textureView.ScaleX = 1;
        }
    }
    internal void UpdateTorch()
    {
        if (cameraView.Camera != null && cameraView.Camera.HasFlashUnit)
        {
            if (started)
            {
                previewBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                previewBuilder.Set(CaptureRequest.FlashMode, cameraView.TorchEnabled ? (int)ControlAEMode.OnAutoFlash : (int)ControlAEMode.Off);
                previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
            }
            else if (initiated)
                cameraManager.SetTorchMode(cameraView.Camera.DeviceId, cameraView.TorchEnabled);
        }
    }
    internal void UpdateFlashMode()
    {
        if (previewSession != null && previewBuilder != null && cameraView.Camera != null && cameraView != null)
        {
            try
            {
                if (cameraView.Camera.HasFlashUnit)
                {
                    switch (cameraView.FlashMode)
                    {
                        case FlashMode.Auto:
                            previewBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
                            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
                            break;
                        case FlashMode.Enabled:
                            previewBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
                            break;
                        case FlashMode.Disabled:
                            previewBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.Off);
                            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
                            break;
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }
    }
    internal void SetZoomFactor(float zoom)
    {
        if (previewSession != null && previewBuilder != null && cameraView.Camera != null)
        {
            //if (OperatingSystem.IsAndroidVersionAtLeast(30))
            //{
            //previewBuilder.Set(CaptureRequest.ControlZoomRatio, Math.Max(Camera.MinZoomFactor, Math.Min(zoom, Camera.MaxZoomFactor)));
            //}
            var destZoom = Math.Clamp(zoom, 1, Math.Min(6, cameraView.Camera.MaxZoomFactor)) - 1;
            Rect m = (Rect)camChars.Get(CameraCharacteristics.SensorInfoActiveArraySize);
            int minW = (int)(m.Width() / (cameraView.Camera.MaxZoomFactor));
            int minH = (int)(m.Height() / (cameraView.Camera.MaxZoomFactor));
            int newWidth = (int)(m.Width() - (minW * destZoom));
            int newHeight = (int)(m.Height() - (minH * destZoom));
            Rect zoomArea = new((m.Width() - newWidth) / 2, (m.Height() - newHeight) / 2, newWidth, newHeight);
            previewBuilder.Set(CaptureRequest.ScalerCropRegion, zoomArea);
            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
        }
    }
    internal void ForceAutoFocus()
    {
        if (previewSession != null && previewBuilder != null && cameraView.Camera != null)
        {
            previewBuilder.Set(CaptureRequest.ControlAfMode, Java.Lang.Integer.ValueOf((int)ControlAFMode.Off));
            previewBuilder.Set(CaptureRequest.ControlAfTrigger, Java.Lang.Integer.ValueOf((int)ControlAFTrigger.Cancel));
            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);
            previewBuilder.Set(CaptureRequest.ControlAfMode, Java.Lang.Integer.ValueOf((int)ControlAFMode.Auto));
            previewBuilder.Set(CaptureRequest.ControlAfTrigger, Java.Lang.Integer.ValueOf((int)ControlAFTrigger.Start));
            previewSession.SetRepeatingRequest(previewBuilder.Build(), null, null);

        }
    }
    private static Size ChooseMaxVideoSize(Size[] choices)
    {
        Size result = choices[0];
        int diference = 0;

        foreach (Size size in choices)
        {
            if (size.Width == size.Height * 4 / 3 && size.Width * size.Height > diference)
            {
                result = size;
                diference = size.Width * size.Height;
            }
        }

        return result;
    }
    private Size ChooseVideoSize(Size[] choices)
    {
        Size result = choices[0];
        int diference = int.MaxValue;
        bool swapped = IsDimensionSwapped();
        foreach (Size size in choices)
        {
            int w = swapped ? size.Height : size.Width;
            int h = swapped ? size.Width : size.Height;
            if (size.Width == size.Height * 4 / 3 && w >= Width && h >= Height && size.Width * size.Height < diference)
            {
                result = size;
                diference = size.Width * size.Height;
            }
        }

        return result;
    }

    private void AdjustAspectRatio(int videoWidth, int videoHeight)
    {
        Matrix txform = new();
        /*
        float scaleX = (float)videoWidth / Width;
        float scaleY = (float)videoHeight / Height;
        bool swapped = IsDimensionSwapped();
        if (swapped)
        {
            scaleX = (float)videoHeight / Width;
            scaleY = (float)videoWidth / Height;
        }
        if (scaleX <= scaleY)
        {
            scaleY /= scaleX;
            scaleX = 1;
        }
        else
        {
            scaleX /= scaleY;
            scaleY = 1;
        }
        */
        RectF viewRect = new(0, 0, Width, Height);
        float centerX = viewRect.CenterX();
        float centerY = viewRect.CenterY();
        RectF bufferRect = new(0, 0, videoHeight, videoWidth);
        bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
        txform.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
        float scale = Math.Max(
                (float)Height / videoHeight,
                (float)Width / videoWidth);
        txform.PostScale(scale, scale, centerX, centerY);

        //txform.PostScale(scaleX, scaleY, centerX, centerY);
        IWindowManager windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        var rotation = windowManager.DefaultDisplay.Rotation;
        if (SurfaceOrientation.Rotation90 == rotation || SurfaceOrientation.Rotation270 == rotation)
        {
            txform.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
        }
        else if (SurfaceOrientation.Rotation180 == rotation)
        {
            txform.PostRotate(180, centerX, centerY);
        }
        textureView.SetTransform(txform);
    }

    protected override async void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        if (started && !recording)
            await StartCameraAsync(cameraView.PhotosResolution);
    }

    private bool IsDimensionSwapped()
    {
        IWindowManager windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        var displayRotation = windowManager.DefaultDisplay.Rotation;
        var chars = cameraManager.GetCameraCharacteristics(cameraView.Camera.DeviceId);
        int sensorOrientation = (int)(chars.Get(CameraCharacteristics.SensorOrientation) as Java.Lang.Integer);
        bool swappedDimensions = false;
        switch (displayRotation)
        {
            case SurfaceOrientation.Rotation0:
            case SurfaceOrientation.Rotation180:
                if (sensorOrientation == 90 || sensorOrientation == 270)
                {
                    swappedDimensions = true;
                }
                break;
            case SurfaceOrientation.Rotation90:
            case SurfaceOrientation.Rotation270:
                if (sensorOrientation == 0 || sensorOrientation == 180)
                {
                    swappedDimensions = true;
                }
                break;
        }
        return swappedDimensions;
    }


    private class MyCameraStateCallback : CameraDevice.StateCallback
    {
        private readonly MauiCameraView cameraView;
        public MyCameraStateCallback(MauiCameraView camView)
        {
            cameraView = camView;
        }
        public override void OnOpened(CameraDevice camera)
        {
            if (camera != null)
            {
                System.Diagnostics.Debug.WriteLine("*****************************************");
                System.Diagnostics.Debug.WriteLine(" MyCameraStateCallBack - OnOpened start");
                System.Diagnostics.Debug.WriteLine("*****************************************");
                cameraView.cameraDevice = camera;
                cameraView.StartPreview();

                System.Diagnostics.Debug.WriteLine("*****************************************");
                System.Diagnostics.Debug.WriteLine(" MyCameraStateCallBack - OnOpened end");
                System.Diagnostics.Debug.WriteLine("*****************************************");
            }
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            camera.Close();
            cameraView.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera?.Close();
            cameraView.cameraDevice = null;
        }
    }

    private class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
    {
        private readonly MauiCameraView cameraView;
        public PreviewCaptureStateCallback(MauiCameraView camView)
        {
            cameraView = camView;
        }
        public override void OnConfigured(CameraCaptureSession session)
        {
            System.Diagnostics.Debug.WriteLine("*****************************************");
            System.Diagnostics.Debug.WriteLine(" PreviewCaptureCallback - OnConfigured start");
            System.Diagnostics.Debug.WriteLine("*****************************************");
            cameraView.previewSession = session;
            cameraView.UpdatePreview();
            System.Diagnostics.Debug.WriteLine("*****************************************");
            System.Diagnostics.Debug.WriteLine(" PreviewCaptureCallback - OnConfigured end");
            System.Diagnostics.Debug.WriteLine("*****************************************");
        }
        public override void OnConfigureFailed(CameraCaptureSession session)
        {
        }
    }
    class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        private readonly MauiCameraView cameraView;

        public ImageAvailableListener(MauiCameraView camView)
        {
            cameraView = camView;
        }
        public void OnImageAvailable(ImageReader reader)
        {
            try
            {
                var image = reader?.AcquireNextImage();
                if (image == null)
                    return;

                var buffer = image.GetPlanes()?[0].Buffer;
                if (buffer == null)
                    return;

                var imageData = new byte[buffer.Capacity()];
                buffer.Get(imageData);
                cameraView.capturePhoto = imageData;
                buffer.Clear();
                image.Close();
            }
            catch
            {
            }
            cameraView.captureDone = true;
        }
    }
}


