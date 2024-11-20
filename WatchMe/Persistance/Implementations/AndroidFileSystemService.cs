#if ANDROID
using Android.Content;
using Android.Provider;
using WatchMe.Repository;

namespace WatchMe.Persistance.Implementations
{
    public class AndroidFileSystemService : BaseFileSystemService
    {
        public AndroidFileSystemService() { }
        public override bool SaveVideoToFileSystem(byte[] videoBytes, string fileName)
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

        public override FileStream GetFileStreamOfFile(string fullFilePath) =>
            new FileStream(fullFilePath, FileMode.Open);
    }
}
#endif
