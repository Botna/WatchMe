namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string fileName);
        FileStream GetFileStreamOfFile(string filename);
    }
}
