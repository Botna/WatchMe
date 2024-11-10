
using WatchMe.Persistance.Implementations;

namespace WatchMe.Repository
{
    public interface IFileSystemServiceFactory
    {
        public IFileSystemService GetVideoRepository();
    }
    public class FileSystemServiceFactory : IFileSystemServiceFactory
    {
        public IFileSystemService GetVideoRepository()
        {
#if ANDROID
            return new AndroidFileSystemService();
#endif

#if IOS
            return new IOSFileSystemService();
#endif
            throw new NotImplementedException();
        }
    }
}
