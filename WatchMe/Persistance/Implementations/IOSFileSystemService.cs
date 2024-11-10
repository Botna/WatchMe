
#if IOS
using WatchMe.Repository;

namespace WatchMe.Persistance.Implementations
{

    internal class IOSFileSystemService : IFileSystemService
    {
        public byte[] LoadVideFromFileSystem(string filename)
        {
            throw new NotImplementedException();
        }

        public bool SaveVideoToFileSystem(byte[] videoBytes, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
#endif