namespace WatchMe.Persistance
{
    public interface IFileSystemService
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string filePath);
        FileStream GetFileStreamOfFile(string filePath);
        Task<byte[]?> GetVideoBytesByFile(string filePath);

        string BuildCacheFileDirectory(string fileName);
    }
}
