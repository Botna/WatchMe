using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using WatchMe.Camera;
using WatchMe.Extensions;
using WatchMe.Pages;
using WatchMe.Persistance;
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

            builder.Services.AddTransient<IOrchestrationService, OrchestrationService>();
            builder.Services.AddTransient<ICloudProviderService, CloudProviderService>();
            builder.Services.AddTransient<INotificationService, NotificationService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            if (DeviceInfo.Name == "ui_automation_emulator")
            {
                ISEMULATED = true;
            }
            return builder.Build();
        }
    }
}
