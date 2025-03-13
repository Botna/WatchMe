using WatchMe.Persistance.Implementations;
using WatchMe.Repository;
using WatchMe.Services;
using WatchMe.Services.Camera;

namespace WatchMe.Extensions
{
    public static class MauiAppBuiderExtensions
    {
        public static void AddPlatformSpecificDependancyInjection(this MauiAppBuilder builder)
        {

#if ANDROID
            builder.Services.AddTransient<IFileSystemService, AndroidFileSystemService>();
            builder.Services.AddTransient<ICameraService, AndroidCameraService>();
            builder.Services.AddTransient<IServiceTest, DemoService>();
#endif

#if IOS
            builder.Services.AddTransient<IFileSystemService, IOSFileSystemService>();
#endif
        }
    }
}
