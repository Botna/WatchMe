#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using WatchMe.Helpers;
using WatchMe.Persistance.CloudProviders;
using WatchMe.Persistance.Sqlite;
using WatchMe.Repository;

namespace WatchMe.Services
{

    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class VideoUploadForegroundService : Service, IVideoUploadForegroundService
    {
        private CancellationTokenSource _cancellationTokenSource;
        //private ConcurrentDictionary<int, long> _videoIdsInProgress = new ConcurrentDictionary<int, long>();

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
            //else if (intent.Action == "STOP_SERVICE")
            //{
            //    //Task.Run(() => StopCameraRecording());
            //    StopForeground(true);//Stop the service
            //    StopSelfResult(startId);
            //}

            return StartCommandResult.NotSticky;
        }

        public async Task UploadActiveChunks()
        {

            //Lets build this as if there is only one active instance of this service.
            //Will have to test an dsee what we can do to make that a reality later.
            //Subsequent SSTART_SERVICE calls are another story, and will need to be handled appropriately.

            //what do we wanna do

            //TODO, this needs to handle an interrupted video, but do this later.

            //look at in progress videos (based on their state being `recording` which should be guaratneed as we launch this service alongside the ).
            //Read portions of them and upload to any configured cloud providers
            //Sleep for a moment in order to not overwhelm the phone possibly, and allow more `data` to be written for the inprogress videos
            //Do this until the videos are marked complete, then finish them off.

            //This will end up with a .ts copy on cloud providers, completely separate than whatever we save to the local file system.


            var secondsToSleep = 5;

            var fileSystemService = CurrentServiceProvider.Services.GetService<IFileSystemService>();
            var videosRepository = CurrentServiceProvider.Services.GetService<IVideosRepository>();
            var cloudProviderService = CurrentServiceProvider.Services.GetService<ICloudProviderService>();

            if (fileSystemService == null || videosRepository == null || cloudProviderService == null)
            {
                throw new Exception("error pulling from DI");
            }


            //files.ForEach(x => _videoIdsInProgress.TryAdd(x.Id, 0));


            //Stopping service does not kill this task.
            var SENTINEL = true;
            while (SENTINEL)
            {
                Thread.Sleep(secondsToSleep * 1000);
                SENTINEL = false;
                var files = await videosRepository.GetAllVideosAsync();
                //Spin, pull bytes of currently recording videos, and start uploading htem in ~5 second increments. 
                foreach (var file in files)
                {

                    if (file.TotalBytes != 0 && file.TotalBytes == file.BytesOffloaded)
                    {
                        //Video is finished recording, and we've uploaded all the bytes.

                        //need to handle cleanup here, or possibly after the while loop.
                        continue;
                    }

                    var bytes = fileSystemService?.GetFileBytesFromCacheDirectory(file.VideoName, file.BytesOffloaded);
                    if (bytes != null && bytes.Length > 0)
                    {
                        SENTINEL = true;
                        await cloudProviderService.AppendContentToCloud(bytes, file.VideoName);
                        await videosRepository.UpdateBytesOffLoadedOfVideo(file.Id, file.BytesOffloaded + bytes.Length);
                    }
                }

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