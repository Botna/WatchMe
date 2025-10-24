namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string filePath);
        Task<bool> SaveImageStreamToFile(Stream imageStream, string filename);
        FileStream GetFileStreamOfFile(string filePath);
        Task<byte[]?> GetVideoBytesByFile(string filePath);




        string BuildCacheFileDirectory(string fileName);
    }
}
