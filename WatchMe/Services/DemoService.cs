#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Camera2;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Camera.MAUI;

namespace WatchMe.Services
{

    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class DemoService : Service, IServiceTest
    {
        public CameraView _frontCameraView = null;
        public CameraView _backCameraView = null;
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
                StopForeground(true);//Stop the service
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }


        public async Task StartCameraRecording()
        {
            var context = Android.App.Application.Context;
            var cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
            var audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
        }


        //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
        //Setting one action to start and one action to stop the foreground service

        public void StartCameras(CameraView frontCamera, CameraView backCamera)
        {
            _frontCameraView = frontCamera;
            _backCameraView = backCamera;
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