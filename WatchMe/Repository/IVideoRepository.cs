namespace WatchMe.Repository
{
    public interface IVideoRepository
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string fileName);
        byte[] LoadVideFromFileSystem(string filename);
    }
}
