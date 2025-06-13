#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Camera.MAUI;
using WatchMe.Persistance;
using WatchMe.Persistance.Implementations;
using WatchMe.Services.Camera;

namespace WatchMe.Services
{

    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class DemoService : Service, IServiceTest
    {
        private string CurrentFileName;
        private ICameraService _cameraService;

        public DemoService()
        {

        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]//we catch the actions intents to know the state of the foreground service
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action == "START_SERVICE")
            {
                RegisterNotification();//Proceed to notify


                Task.Run(() => StartCameraRecording());

            }
            else if (intent.Action == "STOP_SERVICE")
            {
                Task.Run(() => StopCameraRecording());
                StopForeground(true);//Stop the service
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }


        public void StartCameraRecording()
        {
            if (_cameraService == null)
            {
                _cameraService = new AndroidCameraService();
            }

            var time = DateTime.Now.ToString();
            CurrentFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff") + "_backgroundservice.mp4";
            _cameraService.TryStartRecording(CurrentFileName);
        }

        public async Task StopCameraRecording()
        {
            _cameraService.TryStopRecording();

            var orchestrationService = new OrchestrationService(new CloudProviderService(), new AndroidFileSystemService());
            await orchestrationService.ProcessSavedVideoFile(CurrentFileName, FileSystem.Current.CacheDirectory);
            //dispatch the file transfer
        }


        //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
        //Setting one action to start and one action to stop the foreground service

        public void StartCameras()
        {
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(DemoService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);
        }

        public void StopCameras()
        {
            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartService(stopIntent);
        }

        private void RegisterNotification()
        {
            NotificationChannel channel = new NotificationChannel("ServiceChannel", "ServiceDemo", NotificationImportance.Max);
            NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
            Notification notification = new Notification.Builder(this, "ServiceChannel")
               .SetContentTitle("Service Working")
               .SetSmallIcon(Resource.Drawable.abc_btn_check_material)
               .SetOngoing(true)
               .Build();

            StartForeground(100, notification);

        }
    }
}
#endif