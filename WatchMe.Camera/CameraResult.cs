﻿namespace WatchMe.Camera;

public enum CameraResult
{
    Success,
    AccessDenied,
    NoCameraSelected,
    AccessError,
    NoVideoFormatsAvailable,
    NotInitiated,
    NoMicrophoneSelected,
    ResolutionNotAvailable
}
