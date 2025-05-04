namespace WatchMe.Camera.ZXingHelper;

public record BarcodeEventArgs
{
    public BarcodeResult[] Result { get; init; }
}
