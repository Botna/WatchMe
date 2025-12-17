//#if ANDROID
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Runtime;

//namespace WatchMe.Services.ForegroundServices
//{
//    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
//    public class ForegroundServiceDispatcher : Service
//    {
//        private readonly IVideoUploadForegroundService _vufs;
//        public ForegroundServiceDispatcher(IVideoUploadForegroundService videoUploadForegroundService)
//        {
//            _vufs = videoUploadForegroundService;
//        }

//        private CancellationTokenSource _cancellationTokenSource;

//        public override IBinder OnBind(Intent intent)
//        {
//            throw new NotImplementedException();
//        }

//        [return: GeneratedEnum]//we catch the actions intents to know the state of the foreground service
//        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
//        {
//            if (intent.Action == "START_SERVICE")
//            {
//                RegisterNotification();//Proceed to notify

//                _cancellationTokenSource = new CancellationTokenSource();

//                //Issue if we hit this multiple times, and try to start uploading identical chunks.  Need some state blockage or threadsafe containers.
//                Task.Run(() => _vufs., _cancellationTokenSource.Token);

//            }
//            //else if (intent.Action == "STOP_SERVICE")
//            //{
//            //    //Task.Run(() => StopCameraRecording());
//            //    StopForeground(true);//Stop the service
//            //    StopSelfResult(startId);
//            //}

//            return StartCommandResult.NotSticky;
//        }



//        //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
//        //Setting one action to start and one action to stop the foreground service
//        public void StartVUFS()
//        {
//            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(VideoUploadForegroundService));
//            startService.SetAction("START_SERVICE");
//            MainActivity.ActivityCurrent.StartForegroundService(startService);
//        }

//        private void RegisterNotification()
//        {
//            NotificationChannel channel = new NotificationChannel("ServiceChannel", "ServiceDemo", NotificationImportance.Max);
//            NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
//            manager.CreateNotificationChannel(channel);
//            Notification notification = new Notification.Builder(this, "ServiceChannel")
//               .SetContentTitle("Service Working")
//               .SetSmallIcon(Resource.Drawable.abc_btn_check_material)
//               .SetOngoing(true)
//               .SetForegroundServiceBehavior((int)ForegroundService.TypeDataSync)
//               .Build();

//            StartForeground(100, notification);
//        }
//    }
//}
//#endif