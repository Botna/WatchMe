namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string fileName);
        byte[] LoadVideFromFileSystem(string filename);
    }
}
