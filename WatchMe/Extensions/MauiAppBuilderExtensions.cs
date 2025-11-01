using WatchMe.Persistance;
using WatchMe.Persistance.Implementations;

namespace WatchMe.Extensions
{
    public static class MauiAppBuilderExtensions
    {
        public static void AddPlatformSpecificDependancyInjection(this MauiAppBuilder builder)
        {

#if ANDROID
            builder.Services.AddTransient<IFileSystemService, AndroidFileSystemService>();
#endif

#if IOS
            builder.Services.AddTransient<IFileSystemService, IOSFileSystemService>();
#endif
        }
    }
}
