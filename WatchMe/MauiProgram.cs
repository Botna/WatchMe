using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.ScreenRecording;
using System.Diagnostics;
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
                .UseScreenRecording()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android
                       .OnActivityResult((activity, requestCode, resultCode, data) => LogEvent(nameof(AndroidLifecycle.OnActivityResult), requestCode.ToString()))
                       .OnStart((activity) => LogEvent(nameof(AndroidLifecycle.OnStart)))
                       .OnCreate((activity, bundle) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
                       .OnBackPressed((activity) => LogEvent(nameof(AndroidLifecycle.OnBackPressed)) && false)
                       .OnDestroy((activity) => LogEvent(nameof(AndroidLifecycle.OnDestroy)))
                       .OnPause((activity) => LogEvent(nameof(AndroidLifecycle.OnPause)))
                       .OnResume((activity) => LogEvent(nameof(AndroidLifecycle.OnResume)))
                       .OnStop((activity) => LogEvent(nameof(AndroidLifecycle.OnStop))));

#endif
                    static bool LogEvent(string eventName, string type = null)
                    {
                        System.Diagnostics.Debug.WriteLine($"**************************************************************");
                        System.Diagnostics.Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? string.Empty : $" ({type})")}");
                        return true;
                    }
                });

            builder.AddPlatformSpecificDependancyInjection();

            //Platform independant
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<SplitCameraRecordingPage>();
            builder.Services.AddTransient<SettingsPage>();

            builder.Services.AddTransient<IOrchestrationService, OrchestrationService>();
            builder.Services.AddTransient<ICloudProviderService, CloudProviderService>();


            if (DeviceInfo.Name == "ui_automation_emulator")
            {
                ISEMULATED = true;
            }
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");
            Debug.WriteLine("******************** hello ********************");

#if DEBUG
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
