using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace WatchMe
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [Register("com.dbstudios.watchme.MainActivity")]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
