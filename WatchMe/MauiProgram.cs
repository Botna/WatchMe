using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.ScreenRecording;
using WatchMe.Pages;
using WatchMe.Persistance;
using WatchMe.Persistance.Implementations;
using WatchMe.Repository;

namespace WatchMe
{
    public static class MauiProgram
    {
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

            builder.Services.AddTransient<SplitCameraRecordingPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<IFileSystemServiceFactory, FileSystemServiceFactory>();
            builder.Services.AddTransient<ICloudProviderService, CloudProviderService>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
