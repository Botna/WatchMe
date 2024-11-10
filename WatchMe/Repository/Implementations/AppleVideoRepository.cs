#if IOS


namespace WatchMe.Repository.Implementations
{

    internal class AppleVideoRepository : IVideoRepository
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