using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.ScreenRecording;
using System.Diagnostics;
using WatchMe.Camera;
using WatchMe.Extensions;
using WatchMe.Pages;
using WatchMe.Persistance;
using WatchMe.Persistance.Sqlite;
using WatchMe.Services;

namespace WatchMe
{
    public static class MauiProgram
    {
        public static bool ISEMULATED = false;
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCameraView()
                .UseScreenRecording()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.AddPlatformSpecificDependancyInjection();

            //Platform independant
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<SplitCameraRecordingPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<VideosRepository>();
            builder.Services.AddTransient<VideoChunksRepository>();

            builder.Services.AddTransient<IOrchestrationService, OrchestrationService>();
            builder.Services.AddTransient<ICloudProviderService, CloudProviderService>();
            builder.Services.AddTransient<INotificationService, NotificationService>();



            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");

#if DEBUG
            ISEMULATED = true;
            builder.Logging.AddDebug();
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
#endif

            return builder.Build();
        }
    }
}
