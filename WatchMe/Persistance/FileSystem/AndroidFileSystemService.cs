#if ANDROID
using Android.Content;
using Android.Provider;

namespace WatchMe.Persistance.Implementations
{
    public class AndroidFileSystemService : BaseFileSystemService
    {
        public AndroidFileSystemService() { }

        public override async Task<byte[]> MoveVideoToGallery(string fileName)
        {
            var bytes = await GetAllFileBytesFromCacheDirectory(fileName);
            if (bytes?.Length == 0)
            {
                return bytes;
            }
            //use FFMPeg to convert the bytes[] into a proper mp4 video, then save the bytes as an mp4, so my phoen can play it!
            var context = Platform.CurrentActivity;
            var resolver = context.ContentResolver;
            var contentValues = new ContentValues();
            contentValues.Put(MediaStore.IMediaColumns.DisplayName, fileName);
            contentValues.Put(MediaStore.Files.IFileColumns.MimeType, "video/mp2t");
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


    }
}
#endif
