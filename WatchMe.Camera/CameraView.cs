﻿using System.Collections.ObjectModel;



#if IOS || MACCATALYST
using DecodeDataType = UIKit.UIImage;
#elif ANDROID
using DecodeDataType = Android.Graphics.Bitmap;
#elif WINDOWS
using DecodeDataType = Windows.Graphics.Imaging.SoftwareBitmap;
#else
using DecodeDataType = System.Object;
#endif

namespace WatchMe.Camera;

public class CameraView : View, ICameraView
{
    public static readonly BindableProperty SelfProperty = BindableProperty.Create(nameof(Self), typeof(CameraView), typeof(CameraView), null, BindingMode.OneWayToSource);
    public static readonly BindableProperty FlashModeProperty = BindableProperty.Create(nameof(FlashMode), typeof(FlashMode), typeof(CameraView), FlashMode.Disabled);
    public static readonly BindableProperty TorchEnabledProperty = BindableProperty.Create(nameof(TorchEnabled), typeof(bool), typeof(CameraView), false);
    public static readonly BindableProperty CamerasProperty = BindableProperty.Create(nameof(Cameras), typeof(ObservableCollection<CameraInfo>), typeof(CameraView), new ObservableCollection<CameraInfo>());
    public static readonly BindableProperty NumCamerasDetectedProperty = BindableProperty.Create(nameof(NumCamerasDetected), typeof(int), typeof(CameraView), 0);
    public static readonly BindableProperty CameraProperty = BindableProperty.Create(nameof(Camera), typeof(CameraInfo), typeof(CameraView), null, propertyChanged: CameraChanged);
    public static readonly BindableProperty MicrophonesProperty = BindableProperty.Create(nameof(Microphones), typeof(ObservableCollection<MicrophoneInfo>), typeof(CameraView), new ObservableCollection<MicrophoneInfo>());
    public static readonly BindableProperty NumMicrophonesDetectedProperty = BindableProperty.Create(nameof(NumMicrophonesDetected), typeof(int), typeof(CameraView), 0);
    public static readonly BindableProperty MicrophoneProperty = BindableProperty.Create(nameof(Microphone), typeof(MicrophoneInfo), typeof(CameraView), null);
    public static readonly BindableProperty MirroredImageProperty = BindableProperty.Create(nameof(MirroredImage), typeof(bool), typeof(CameraView), false);
    public static readonly BindableProperty ZoomFactorProperty = BindableProperty.Create(nameof(ZoomFactor), typeof(float), typeof(CameraView), 1f);
    public static readonly BindableProperty AutoStartPreviewProperty = BindableProperty.Create(nameof(AutoStartPreview), typeof(bool), typeof(CameraView), false, propertyChanged: AutoStartPreviewChanged);
    public static readonly BindableProperty AutoRecordingFileProperty = BindableProperty.Create(nameof(AutoRecordingFile), typeof(string), typeof(CameraView), string.Empty);
    public static readonly BindableProperty AutoStartRecordingProperty = BindableProperty.Create(nameof(AutoStartRecording), typeof(bool), typeof(CameraView), false, propertyChanged: AutoStartRecordingChanged);

    /// <summary>
    /// Binding property for use this control in MVVM.
    /// </summary>
    public CameraView Self
    {
        get { return (CameraView)GetValue(SelfProperty); }
        set { SetValue(SelfProperty, value); }
    }
    /// <summary>
    /// Flash mode for take a photo. This is a bindable property.
    /// </summary>
    public FlashMode FlashMode
    {
        get { return (FlashMode)GetValue(FlashModeProperty); }
        set { SetValue(FlashModeProperty, value); }
    }
    /// <summary>
    /// Turns the camera torch on and off if available. This is a bindable property.
    /// </summary>
    public bool TorchEnabled
    {
        get { return (bool)GetValue(TorchEnabledProperty); }
        set { SetValue(TorchEnabledProperty, value); }
    }
    /// <summary>
    /// List of available cameras in the device. This is a bindable property.
    /// </summary>
    public ObservableCollection<CameraInfo> Cameras
    {
        get { return (ObservableCollection<CameraInfo>)GetValue(CamerasProperty); }
        set { SetValue(CamerasProperty, value); }
    }
    /// <summary>
    /// Indicates the number of available cameras in the device.
    /// </summary>
    public int NumCamerasDetected
    {
        get { return (int)GetValue(NumCamerasDetectedProperty); }
        set { SetValue(NumCamerasDetectedProperty, value); }
    }
    /// <summary>
    /// Set the camera to use by the controler. This is a bindable property.
    /// </summary>
    public CameraInfo Camera
    {
        get { return (CameraInfo)GetValue(CameraProperty); }
        set { SetValue(CameraProperty, value); }
    }
    /// <summary>
    /// List of available microphones in the device. This is a bindable property.
    /// </summary>
    public ObservableCollection<MicrophoneInfo> Microphones
    {
        get { return (ObservableCollection<MicrophoneInfo>)GetValue(MicrophonesProperty); }
        set { SetValue(MicrophonesProperty, value); }
    }
    /// <summary>
    /// Indicates the number of available microphones in the device.
    /// </summary>
    public int NumMicrophonesDetected
    {
        get { return (int)GetValue(NumMicrophonesDetectedProperty); }
        set { SetValue(NumMicrophonesDetectedProperty, value); }
    }
    /// <summary>
    /// Set the microphone to use by the controler. This is a bindable property.
    /// </summary>
    public MicrophoneInfo Microphone
    {
        get { return (MicrophoneInfo)GetValue(MicrophoneProperty); }
        set { SetValue(MicrophoneProperty, value); }
    }
    /// <summary>
    /// Turns a mirror image of the camera on and off. This is a bindable property.
    /// </summary>
    public bool MirroredImage
    {
        get { return (bool)GetValue(MirroredImageProperty); }
        set { SetValue(MirroredImageProperty, value); }
    }

    /// <summary>
    /// The zoom factor for the current camera in use. This is a bindable property.
    /// </summary>
    public float ZoomFactor
    {
        get { return (float)GetValue(ZoomFactorProperty); }
        set { SetValue(ZoomFactorProperty, value); }
    }
    /// <summary>
    /// Indicates the minimum zoom factor for the camera in use. This property is refreshed when the "Camera" property change.
    /// </summary>
    public float MinZoomFactor
    {
        get
        {
            if (Camera != null)
                return Camera.MinZoomFactor;
            else
                return 1f;
        }
    }
    /// <summary>
    /// Indicates the maximum zoom factor for the camera in use. This property is refreshed when the "Camera" property change.
    /// </summary>
    public float MaxZoomFactor
    {
        get
        {
            if (Camera != null)
                return Camera.MaxZoomFactor;
            else
                return 1f;
        }
    }

    /// <summary>
    /// Starts/Stops the Preview if camera property has been set
    /// </summary>
    public bool AutoStartPreview
    {
        get { return (bool)GetValue(AutoStartPreviewProperty); }
        set { SetValue(AutoStartPreviewProperty, value); }
    }
    /// <summary>
    /// Full path to file where record video will be recorded.
    /// </summary>
    public string AutoRecordingFile
    {
        get { return (string)GetValue(AutoRecordingFileProperty); }
        set { SetValue(AutoRecordingFileProperty, value); }
    }
    /// <summary>
    /// Starts/Stops record video to AutoRecordingFile if camera and microphone properties have been set
    /// </summary>
    public bool AutoStartRecording
    {
        get { return (bool)GetValue(AutoStartRecordingProperty); }
        set { SetValue(AutoStartRecordingProperty, value); }
    }
    /// <summary>
    /// Event launched when "Cameras" property has been loaded.
    /// </summary>
    public event EventHandler CamerasLoaded;
    /// <summary>
    /// Event launched when "Microphones" property has been loaded.
    /// </summary>
    public event EventHandler MicrophonesLoaded;
    /// <summary>
    /// A static reference to the last CameraView created.
    /// </summary>
    public static CameraView Current { get; set; }

    internal DateTime lastSnapshot = DateTime.Now;
    internal Size PhotosResolution = new(0, 0);

    public CameraView()
    {
        HandlerChanged += CameraView_HandlerChanged;
        Current = this;
    }
    private void CameraView_HandlerChanged(object sender, EventArgs e)
    {
        if (Handler != null)
        {
            CamerasLoaded?.Invoke(this, EventArgs.Empty);
            MicrophonesLoaded?.Invoke(this, EventArgs.Empty);
            Self = this;
        }
    }

    private static void CameraChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != null && oldValue != newValue && bindable is CameraView cameraView && newValue is CameraInfo)
        {
            cameraView.OnPropertyChanged(nameof(MinZoomFactor));
            cameraView.OnPropertyChanged(nameof(MaxZoomFactor));
        }
    }

    private static async void AutoStartPreviewChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue && bindable is CameraView control)
        {
            try
            {
                if ((bool)newValue)
                    await control.StartCameraAsync();
                else
                    await control.StopCameraAsync();
            }
            catch { }

        }
    }
    private static async void AutoStartRecordingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue && bindable is CameraView control)
        {
            try
            {
                if ((bool)newValue)
                {
                    if (!string.IsNullOrEmpty(control.AutoRecordingFile))
                        await control.StartRecordingAsync(control.AutoRecordingFile);
                }
                else
                    await control.StopRecordingAsync();
            }
            catch { }

        }
    }

    /// <summary>
    /// Start playback of the selected camera async. "Camera" property must not be null.
    /// <paramref name="Resolution"/> Indicates the resolution for the preview and photos taken with TakePhotoAsync (must be in Camera.AvailableResolutions). If width or height is 0, max resolution will be taken.
    /// </summary>
    public async Task<CameraResult> StartCameraAsync(Size Resolution = default)
    {
        CameraResult result = CameraResult.AccessError;
        if (Camera != null)
        {
            PhotosResolution = Resolution;
            if (Resolution.Width != 0 && Resolution.Height != 0)
            {
                if (!Camera.AvailableResolutions.Any(r => r.Width == Resolution.Width && r.Height == Resolution.Height))
                    return CameraResult.ResolutionNotAvailable;
            }
            if (Handler != null && Handler is CameraViewHandler handler)
            {
                result = await handler.StartCameraAsync(Resolution);
                if (result == CameraResult.Success)
                {

                    OnPropertyChanged(nameof(MinZoomFactor));
                    OnPropertyChanged(nameof(MaxZoomFactor));
                }
            }
        }
        else
            result = CameraResult.NoCameraSelected;

        return result;
    }
    /// <summary>
    /// Start recording a video async. "Camera" property must not be null.
    /// <paramref name="file"/> Full path to file where video will be stored.
    /// <paramref name="Resolution"/> Sets the Video Resolution. It must be in Camera.AvailableResolutions. If width or height is 0, max resolution will be taken.
    /// </summary>
    public async Task<CameraResult> StartRecordingAsync(string file, Size Resolution = default)
    {
        CameraResult result = CameraResult.AccessError;
        if (Camera != null)
        {
            if (Resolution.Width != 0 && Resolution.Height != 0)
            {
                if (!Camera.AvailableResolutions.Any(r => r.Width == Resolution.Width && r.Height == Resolution.Height))
                    return CameraResult.ResolutionNotAvailable;
            }
            if (Handler != null && Handler is CameraViewHandler handler)
            {
                result = await handler.StartRecordingAsync(file, Resolution);
                if (result == CameraResult.Success)
                {

                    OnPropertyChanged(nameof(MinZoomFactor));
                    OnPropertyChanged(nameof(MaxZoomFactor));
                }
            }
        }
        else
            result = CameraResult.NoCameraSelected;

        return result;
    }
    /// <summary>
    /// Stop playback of the selected camera async.
    /// </summary>
    public async Task<CameraResult> StopCameraAsync()
    {
        CameraResult result = CameraResult.AccessError;
        if (Handler != null && Handler is CameraViewHandler handler)
        {
            result = await handler.StopCameraAsync();
        }
        return result;
    }
    /// <summary>
    /// Stop recording a video async.
    /// </summary>
    public async Task<CameraResult> StopRecordingAsync()
    {
        CameraResult result = CameraResult.AccessError;
        if (Handler != null && Handler is CameraViewHandler handler)
        {
            result = await handler.StopRecordingAsync();
        }
        return result;
    }

    /// <summary>
    /// Force execute the camera autofocus trigger.
    /// </summary>
    public void ForceAutoFocus()
    {
        if (Handler != null && Handler is CameraViewHandler handler)
        {
            handler.ForceAutoFocus();
        }
    }
    /// <summary>
    /// Forces the device specific control dispose
    /// </summary>
    public void ForceDisposeHandler()
    {
        if (Handler != null && Handler is CameraViewHandler handler)
        {
            handler.ForceDispose();
        }
    }
    internal void RefreshDevices()
    {
        Task.Run(() =>
        {
            OnPropertyChanged(nameof(Cameras));
            NumCamerasDetected = Cameras.Count;
            OnPropertyChanged(nameof(Microphones));
            NumMicrophonesDetected = Microphones.Count;
        });
    }
    public static async Task<bool> RequestPermissions(bool withMic = false, bool withStorageWrite = false)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) return false;
        }
        if (withMic)
        {
            status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
                if (status != PermissionStatus.Granted) return false;
            }
        }
        if (withStorageWrite)
        {
            status = await Permissions.CheckStatusAsync<Permissions.Media>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.CheckStatusAsync<Permissions.Media>();
                if (status != PermissionStatus.Granted)
                {
                    PermissionStatus status1 = await Permissions.RequestAsync<Permissions.Media>();
                    if (status1 != PermissionStatus.Granted) return false;
                }
            }
        }
        return true;
    }
}