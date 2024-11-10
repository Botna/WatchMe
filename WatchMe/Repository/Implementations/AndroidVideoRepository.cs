#if ANDROID
using Android.Content;
using Android.Provider;

namespace WatchMe.Repository.Implementations
{
    public class AndroidVideoRepository : IVideoRepository
    {
        public AndroidVideoRepository() { }
        public bool SaveVideoToFileSystem(byte[] videoBytes, string fileName)
        {
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
                output.Write(videoBytes, 0, videoBytes.Length);
                output.Flush();
                output.Close();
                output.Dispose();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return false;
            }

            //wahts this do?
            //contentValues.Put(MediaStore.IMediaColumns.IsPending, 1);
            return true;
        }

        public byte[] LoadVideFromFileSystem(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
