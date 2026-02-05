#if ANDROID
using Android.Content;
using Android.Provider;
using LibVLCSharp.Shared;


namespace WatchMe.Persistance.Implementations
{
    public class AndroidFileSystemService : BaseFileSystemService
    {
        public AndroidFileSystemService()
        {
        }

        public override async Task<byte[]> MoveVideoToGallery(string fileName)
        {
            //var bytes = await GetAllFileBytesFromCacheDirectory(fileName);
            //if (bytes?.Length == 0)
            //{
            //    return bytes;
            //}

            var bytes = await PullVideoAndConvert(fileName);


            var context = Platform.CurrentActivity;
            var resolver = context.ContentResolver;
            var contentValues = new ContentValues();
            contentValues.Put(MediaStore.IMediaColumns.DisplayName, fileName);
            contentValues.Put(MediaStore.Files.IFileColumns.MimeType, "video/mp4");
            contentValues.Put(MediaStore.IMediaColumns.RelativePath, "DCIM/WatchMeVideoCaptures");
            try
            {
                var videoUri = resolver.Insert(MediaStore.Video.Media.ExternalContentUri, contentValues);
                var output = resolver.OpenOutputStream(videoUri);
                output.Write(bytes, 0, bytes.Length);
                output.Flush();
                output.Close();
                output.Dispose();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return Array.Empty<byte>();
            }
            return bytes;
        }

        private async Task<byte[]> PullVideoAndConvert(string fileName)
        {
            using var libvlc = new LibVLC(enableDebugLogs: true);
            using var media = new Media(libvlc, new Uri(BuildCacheFileDirectory(fileName)));
            return null;
        }



    }
}
#endif
