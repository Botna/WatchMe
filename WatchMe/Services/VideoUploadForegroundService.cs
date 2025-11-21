#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System.Collections.Concurrent;
using WatchMe.Helpers;
using WatchMe.Persistance.Sqlite;
using WatchMe.Repository;

namespace WatchMe.Services
{

    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class VideoUploadForegroundService : Service, IVideoUploadForegroundService
    {
        private CancellationTokenSource _cancellationTokenSource;

        private ConcurrentBag<int> _videoIdsInProgress = new ConcurrentBag<int>();
        public VideoUploadForegroundService()
        { }

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

                _cancellationTokenSource = new CancellationTokenSource();

                //Issue if we hit this multiple times, and try to start uploading identical chunks.  Need some state blockage or threadsafe containers.
                Task.Run(() => UploadActiveChunks(), _cancellationTokenSource.Token);

            }
            else if (intent.Action == "STOP_SERVICE")
            {
                //Task.Run(() => StopCameraRecording());
                StopForeground(true);//Stop the service
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }

        public async Task UploadActiveChunks()
        {
            //what do we wanna do

            //TODO, this needs to handle an interrupted video, but do this later.

            //look at in progress videos (based on their state being `recording` which should be guaratneed as we launch this service alongside the ).
            //Read portions of them and upload to any configured cloud providers
            //Sleep for a moment in order to not overwhelm the phone possibly, and allow more `data` to be written for the inprogress videos
            //Do this until the videos are marked complete, then finish them off.

            //This will end up with a .ts copy on cloud providers, completely separate than whatever we save to the local file system.

            var fileSystemService = CurrentServiceProvider.Services.GetService<IFileSystemService>();
            var videosRepository = CurrentServiceProvider.Services.GetService<IVideosRepository>();

            //Video chunks may be more of a `interuption` thing, or if we have to tear down this task and start a new one with multiple SERVICE_START's
            //Right now, just the video files may be sufficient, so long as they get closed out correctly.
            //var videoChunksRepository = CurrentServiceProvider.Services.GetService<IVideoChunksRepository>();

            var files = await videosRepository.GetAllVideosAsync();

            //var fileChunks = await videoChunksRepository.GetVideoChunksByVideoIdsAsync(files.Select(x => x.Id));

            //this should represent all the in progress ids that we need to worry about.  This Service is kicked off when videos are in progress of being recorded, so we can pretty much
            // be sure that well have our appropraite videoId's at our disposal when this launches.

            //Subsequent SSTART_SERVICE calls are another story, and will need to be handled appropriately.

            files.ForEach(x => _videoIdsInProgress.Add(x.Id));

            var secondsToSleep = 5;

            //Stopping service does not kill this task.
            var SENTINEL = true;
            while (SENTINEL)
            {
                Thread.Sleep(secondsToSleep * 1000);
                //Spin, pull bytes of currently recording videos, and start uploading htem in ~5 second increments. 
                foreach (var file in files)
                {

                    //do something


                }
                SENTINEL = false;
            }



        }

        //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
        //Setting one action to start and one action to stop the foreground service
        public void StartVUFS()
        {
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(VideoUploadForegroundService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartForegroundService(startService);
        }

        public void StopVUFS()
        {
            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, typeof(VideoUploadForegroundService));
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartForegroundService(stopIntent);
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
               .SetForegroundServiceBehavior((int)ForegroundService.TypeDataSync)
               .Build();

            StartForeground(100, notification);

        }
    }
}
#endif