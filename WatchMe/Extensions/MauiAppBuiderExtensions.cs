using WatchMe.Persistance.Implementations;
using WatchMe.Repository;

namespace WatchMe.Extensions
{
    public static class MauiAppBuiderExtensions
    {
        public static void AddPlatformSpecificDependancyInjection(this MauiAppBuilder builder)
        {

#if ANDROID
            builder.Services.AddTransient<IFileSystemService, AndroidFileSystemService>();
#endif

#if IOS
            //builder.Services.AddTransient<IFileSystemService, IOSFileSystemService>();
#endif
        }
    }
}
